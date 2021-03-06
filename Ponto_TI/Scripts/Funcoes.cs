﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Net.NetworkInformation;

namespace Ponto_TI.scripts
{
    public class Funcoes
    {
        //declaração de variáveis
        private SqlConnection connection;      
        private string uid;
        private string password;
        private StreamWriter Arqlog;        
        public int ContColab, ContPonto, IdUsuario, IdGrupoUsuario, ContLogin;
        public string strIdUsuario, strIdGrupoUsuario, Valor, StatusLogin, MsgRetorno;
                
        public void Conecta_SQL()
        {
            Inicializa();
        }

        private void Inicializa()
        {
            /*Atribuindo valores às variáveis que irão definir a string de conexão e cria arquivo de log*/                       
            uid = "pontoti";
            password = "pontoadmin";
            string ConnStr;            
            ConnStr = "Data Source=localhost;User ID=" + uid + ";Password = "+ password +"";
            //connection.ConnectionString = ConnStr;
            connection = new SqlConnection(ConnStr);          
            
            CriaLog();
        }

        public void CriaLog()
        {
            //Criando Arquivo de Log
            using (Arqlog = File.AppendText("E:\\log.txt"))
            {
                Arqlog.WriteLine("Conexão com Banco de Dados SQL Server ");
                Arqlog.WriteLine("==============================================");
                Arqlog.WriteLine("Data/Hora:" + DateTime.Now.ToString());               
            }
        }

        public void EscreveLog(string Mensagem, TextWriter Escrita)
        {
            Escrita.WriteLine(Mensagem);
        }

        //Abre a Conexão com o Banco de Dados Oracle
        private bool AbreConexao()
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                    using (StreamWriter w = File.AppendText("E:\\log.txt"))
                    {
                        EscreveLog(DateTime.Now.ToString() + " - " + "Conexão efetuada com sucesso!", w);
                    }
                }                
                return true;
            }
            catch (SqlException ex)
            {
                //Verifica o erro durante a conexão e grava no log, os erros mais comuns são: 
                
                switch (ex.ErrorCode)
                {
                    case 0:
                        using (StreamWriter w = File.AppendText("E:\\log.txt"))
                        {
                            EscreveLog(DateTime.Now.ToString() + " - " + "Falha ao conectar o servidor, contate o administrador!", w);
                        }                        
                        break;

                    case 1045:
                        using (StreamWriter w = File.AppendText("E:\\log.txt"))
                        {
                            EscreveLog(DateTime.Now.ToString() + " - " + "Usuário ou Senha Inválidos!", w);
                        }                
                        break;
                    case 4060:
                        using (StreamWriter w = File.AppendText("E:\\log.txt"))
                        {
                            EscreveLog(DateTime.Now.ToString() + " - " + "O usuário não possui permissão de acesso à base de dados especificada!", w);
                        }
                        break;
                    case 40532:
                        using (StreamWriter w = File.AppendText("E:\\log.txt"))
                        {
                            EscreveLog(DateTime.Now.ToString() + " - " + "Não foi possível conectar ao servidor com o login especificado!", w);
                        }
                        break;
                }
                return false;
            }
        }

        //Fecha conexão com Banco de Dados
        private bool FechaConexao()
        {
            try
            {
                connection.Close();
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Conexão finalizada com sucesso!", w);
                }                
                return true;
            }
            catch (SqlException ex)
            {
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Erro ao encerrar conexão!", w);
                    EscreveLog(DateTime.Now.ToString() + " - " + ex.Message, w);
                }                
                return false;
            }
        }

        //Pesquisa o CPF na base de dados
        public void SelectCPF(string cpf)
        {           
            try
            {
                //String para pesquisa
                string query = "SELECT DISTINCT id_colab FROM tbl_colab WHERE cpf_colab='" + cpf + "'";
                
                //Criando Lista para Armazenar os Dados do Select
                List<string>[] Lista = new List<string>[1];
                Lista[0] = new List<string>();

                if (this.AbreConexao() == true)
                {                    
                    //Cria Comando
                    SqlCommand cmd = new SqlCommand(query, connection);

                    //Criando o data Reader
                    SqlDataReader SQL_DR = cmd.ExecuteReader();
                    DataTable SQL_DT = new DataTable();
                    SQL_DT.Load(SQL_DR);
                    
                    ContColab = SQL_DT.Rows.Count;
                    
                    
                    if (ContColab == 1)
                    {
                        MsgRetorno = "CPF CADASTRADO";
                        Valor = SQL_DT.Rows[0]["id_colab"].ToString();                        
                    }
                    else if (ContColab == 0)
                    {
                        MsgRetorno = "CPF NAO CADASTRADO";               
                    }
                    //Fecha Data Reader
                    SQL_DR.Close();

                    //Fecha Conexão com Banco de Dados
                    this.FechaConexao();                    
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Erro ao realizar Select na tabela tbl_colab", w);
                    EscreveLog(DateTime.Now.ToString() + " - " + ex.Message, w);
                }
            }            
        }

        //Pesquisa o login na base de dados
        public void SelectLogin(string login, string senha)
        {
            try
            {
                //String para pesquisa
                string query = "SELECT DISTINCT id_colab, grplogin_colab FROM tbl_colab WHERE usr_colab='" + login + "' AND psw_colab= '" + senha + "'";
            
                //Criando Lista para Armazenar os Dados do Select

                List<int>[] Lista = new List<int>[2];
                Lista[0] = new List<int>();
                Lista[1] = new List<int>();

                if (this.AbreConexao() == true)
                {
                    //Cria Comando
                    SqlCommand cmd = new SqlCommand(query, connection);

                    //Criando o data Reader
                    SqlDataReader SQL_DR = cmd.ExecuteReader();

                    DataTable DtLogin = new DataTable();
                    DtLogin.Load(SQL_DR);
                    ContLogin = DtLogin.Rows.Count;

                    if (ContLogin != 1)
                    {
                        StatusLogin = "Erro";
                    }
                    else
                    {
                        strIdUsuario = Convert.ToString(DtLogin.Rows[0]["id_colab"]);
                        strIdGrupoUsuario = Convert.ToString(DtLogin.Rows[0]["grplogin_colab"]);
                        StatusLogin = "Sucesso";
                        DtLogin.Clear();                        
                    }
                
                    //Fecha Data Reader
                    SQL_DR.Close();

                    //Fecha Conexão com Banco de Dados
                    this.FechaConexao();            
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Erro ao realizar Select na tabela tbl_login", w);
                    EscreveLog(DateTime.Now.ToString() + " - " + ex.Message, w);
                }
            }
        }

        //Verifica se o ponto vai ser registrado mais de uma vez com a mesma ação
        public void SelectPonto(string cpf, int acao, int id_colab, int cod_acao, string ipHost)
        {
            try
            {
                //String para pesquisa
                string query = "SELECT DISTINCT id_colab_ponto FROM tbl_ponto AS a inner join tbl_colab as b  on b.id_colab = a.id_colab_ponto WHERE b.cpf_colab = '" + cpf + "' and a.data_ponto >= '" + DateTime.Today.ToString("d")+ "' and a.id_acao_ponto='" + acao + "'";

                //Criando Lista para Armazenar os Dados do Select
                List<string>[] Lista = new List<string>[1];
                Lista[0] = new List<string>();

                if (this.AbreConexao() == true)
                {
                    ContPonto = 0;
                    //Cria Comando
                    SqlCommand cmd = new SqlCommand(query, connection);

                    //Criando o data Reader
                    SqlDataReader SQL_DR = cmd.ExecuteReader();
                    DataTable SQL_DT = new DataTable();
                    SQL_DT.Load(SQL_DR);

                    ContPonto = SQL_DT.Rows.Count;


                    if (ContPonto == 1)
                    {
                        MsgRetorno = "Ponto já registrado";
                    }
                    else
                    {
                        InserePonto(id_colab, cod_acao, ipHost);
                    }
                    //Fecha Data Reader
                    SQL_DR.Close();

                    //Fecha Conexão com Banco de Dados
                    this.FechaConexao();
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Erro ao realizar Select na tabela tbl_colab", w);
                    EscreveLog(DateTime.Now.ToString() + " - " + ex.Message, w);
                }
            }
        }

        //Insere Ponto
        public void InserePonto( int id_colab, int cod_acao, string ipHost)
        {
            string query = "INSERT INTO tbl_ponto VALUES(" + id_colab + ", '" + DateTime.Now.ToString() + "', " + cod_acao + ",'"+ ipHost +"')";
            
            try
            {
                if (this.AbreConexao() == true)
                {
                    //Executa a query definida na variável "query" definida acima
                    SqlCommand cmd = new SqlCommand(query, connection);

                    //Executa comando
                    cmd.ExecuteNonQuery();

                    //Fecha Conexão
                    this.FechaConexao();

                    MsgRetorno = "Ponto registrado com sucesso";
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Erro ao realizar insert na tabela de ponto", w);
                    EscreveLog(DateTime.Now.ToString() + " - " + ex.Message, w);
                }                
                throw;
            }            
        }

        //Insere Colaborador
        public void InsereColab(string ColabNome, string ColabCPF, string usr_colab, string psw_colab, int grplogin, int status_colab, int regional_colab)
        {
            try
            {
                bool StatusCPF = IsCpf(ColabCPF);
                
                if (StatusCPF == true)
                {
                    using (StreamWriter w = File.AppendText("E:\\log.txt"))
                    {
                        EscreveLog(DateTime.Now.ToString() + " - " + "Status do CPF = " + StatusCPF, w);
                    }

                    SelectCPF(ColabCPF);
                    if (MsgRetorno == "CPF NAO CADASTRADO")

                        if (this.AbreConexao() == true)
                        {                            
                            string query = "INSERT INTO TBL_COLAB ([usr_colab],[psw_colab],[grplogin_colab],[nome_colab],[cpf_colab],[status_colab],[regional_colab]) VALUES('" + usr_colab +"', '"+ psw_colab +"', " + grplogin + ", '" + ColabNome.Trim() + "','" + ColabCPF.Trim() + "', "+ status_colab + ", " + regional_colab + ")";

                            //Executa a query definida na variável "query" definida acima
                            SqlCommand cmd = new SqlCommand(query, connection);

                            //Executa comando
                            cmd.ExecuteNonQuery();

                            //Fecha Conexão
                            this.FechaConexao();
                        }
                }
                else
                {
                    using (StreamWriter w = File.AppendText("E:\\log.txt"))
                    {
                        EscreveLog(DateTime.Now.ToString() + " - " + "Status do CPF = " + StatusCPF, w);
                    }
                }               
            }
            catch (Exception ex)
            {
                using (StreamWriter w = File.AppendText("E:\\log.txt"))
                {
                    EscreveLog(DateTime.Now.ToString() + " - " + "Erro ao inserir colaborador", w);
                    EscreveLog(DateTime.Now.ToString() + " - " + ex.Message, w);
                }
                throw;
            }
        }        

        //Update statement
        public void Update()
        {
            string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            if (this.AbreConexao() == true)
            {
                //Executa a query definida na variável "query" definida acima
                SqlCommand cmd = new SqlCommand(query, connection);

                //Executa comando
                cmd.ExecuteNonQuery();

                //Fecha Conexão
                this.FechaConexao();
            }
        }

        //Delete statement
        public void Delete()
        {
            string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.AbreConexao() == true)
            {
                //Executa a query definida na variável "query" definida acima
                SqlCommand cmd = new SqlCommand(query, connection);

                //Executa comando
                cmd.ExecuteNonQuery();

                //Fecha Conexão
                this.FechaConexao();
            }
        }

        //Validação de CPF
        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCpf = tempCpf + digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }    
}
}