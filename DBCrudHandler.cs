using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DataBaseHandler.Util
{
    public class DBCrudHandler<T> where T : class
    {
        #region private var
        private string _connectionString;
        private int resultado;
        private DataTable dt;
        #endregion

        #region public var
        public string connectionsString { get { return _connectionString; } set { _connectionString = value; } }
        #endregion

        #region CRUD

        /// <summary>
        /// Executa uma query passada via string
        /// </summary>
        /// <param name="Query">string</param>
        /// <returns>bool</returns>
        /// <rewards>
        /// Criado por: Luiz Lemos
        /// Criado em: 04/10/2017
        /// </rewards>
        public bool ExecuteQueryNr(string Query)
        {
            try
            {
                using (var context = new SqlConnection(_connectionString))
                {
                    context.Open();
                    SqlCommand sqlCommand = new SqlCommand(Query, context);
                    resultado = sqlCommand.ExecuteNonQuery();
                    context.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (resultado >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Executa uma query passada via string
        /// </summary>
        /// <param name="Query">string</param>
        /// <returns>datatable</returns>
        /// <rewards>
        /// Criado por: Luiz Lemos
        /// Criado em: 06/10/2017
        /// </rewards>
        public DataTable Select(string Query)
        {

            try
            {
                dt = new DataTable();
                using (var context = new SqlConnection(_connectionString))
                {
                    context.Open();
                    SqlCommand sqlCommand = new SqlCommand(Query, context);
                    dt.Load(sqlCommand.ExecuteReader());
                    context.Close();

                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dt = null;
            }


        }

        /// <summary>
        /// Executa um insert através dos dados em uma class
        /// </summary>
        /// <param name="objectClass">Classe</param>
        /// <returns>bool</returns>
        /// <rewards>
        /// Criado por: Luiz Lemos
        /// Criado em: 04/10/2017
        /// </rewards>
        public bool Create(T objectClass)
        {
            string Table = string.Empty;
            BindingFlags flags = new BindingFlags();
            flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var objClassFields = objectClass.GetType().GetFields(flags);
            List<string> parameters = new List<string>();
            List<object> parametersValues = new List<object>();
            string complement = string.Empty;
            string name = "dbo." + objectClass.GetType().Name;
            Table = name;
            string query = "INSERT INTO " + Table + " WITH (NOLOCK) (";

            for (int count = 0; count < objClassFields.Length; count++)
            {
                FieldInfo fields = objClassFields[count];
                if (fields.FieldType == typeof(Int64) || fields.FieldType == typeof(int))
                {
                    Int32 fieldNameLength = fields.Name.IndexOf(">") - (fields.Name.IndexOf("<") + 1);
                    string fieldName = fields.Name.Substring(fields.Name.IndexOf("<") + 1, fieldNameLength);
                    object fieldValue = fields.GetValue(objectClass);
                    if (fieldName.Equals("id", StringComparison.InvariantCultureIgnoreCase) && fields.GetValue(objectClass).ToString() != "0")
                    {
                        parametersValues.Add(fieldValue);
                        if ((count < objClassFields.Length) && (objClassFields.Length - count) == 1)
                        {
                            parameters.Add("@" + fieldName);
                        }
                        else
                        {
                            parameters.Add("@" + fieldName + ",");
                        }


                        if (count < objClassFields.Length && (objClassFields.Length - count) == 1)
                        {
                            query += fieldName;
                        }
                        else
                        {
                            query += fieldName + ",";
                        }

                        if (count < objClassFields.Length && (objClassFields.Length - count) == 1)
                        {
                            complement += "@" + fieldName;
                        }
                        else
                        {
                            complement += "@" + fieldName + ",";
                        }
                    }
                }

                if (fields.FieldType == typeof(string))
                {
                    Int32 fieldNameLength = fields.Name.IndexOf(">") - (fields.Name.IndexOf("<") + 1);
                    string fieldName = fields.Name.Substring(fields.Name.IndexOf("<") + 1, fieldNameLength);
                    var fieldValue = fields.GetValue(objectClass);
                    parametersValues.Add(fieldValue);

                    if ((count < objClassFields.Length) && (objClassFields.Length - count) == 1)
                    {
                        parameters.Add("@" + fieldName);
                    }
                    else
                    {
                        parameters.Add("@" + fieldName + ",");
                    }


                    if (count < objClassFields.Length && (objClassFields.Length - count) == 1)
                    {
                        query += fieldName;
                    }
                    else
                    {
                        query += fieldName + ",";
                    }

                    if (count < objClassFields.Length && (objClassFields.Length - count) == 1)
                    {
                        complement += "@" + fieldName;
                    }
                    else
                    {
                        complement += "@" + fieldName + ",";
                    }

                }

            }

            query += ") VALUES (";
            complement += ")";
            query += complement;

            try
            {
                using (var context = new SqlConnection(_connectionString))
                {
                    context.Open();

                    SqlCommand sqlCommand = new SqlCommand(query, context);
                    for (int count = 0; count < parameters.Count; count++)
                    {
                        sqlCommand.Parameters.AddWithValue(parameters.ElementAt(count).ToString(), parametersValues.ElementAt(count).ToString());//objectClass.GetType().GetFields().ElementAt(count).GetValue(objectClass.GetType().GetFields()));
                    }
                    resultado = sqlCommand.ExecuteNonQuery();
                    context.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (resultado >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Executa um update através dos dados em uma class
        /// </summary>
        /// <param name="objectClass">Classe</param>
        /// <returns>bool</returns>
        /// <rewards>
        /// Criado por: Luiz Lemos
        /// Criado em: 06/10/2017
        /// </rewards>
        public bool Update(T objectClass)
        {
            FieldInfo fields;
            string Table = string.Empty;
            BindingFlags flags = new BindingFlags();
            flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var objClassFields = objectClass.GetType().GetFields(flags);
            List<string> parameters = new List<string>();
            List<object> parametersValues = new List<object>();
            string complement = string.Empty;
            string name = "dbo." + objectClass.GetType().Name;
            Table = name;
            string query = "UPDATE " + Table + " SET ";

            for (int count = 0; count < objClassFields.Length; count++)
            {
                fields = objClassFields[count];
                if (fields.FieldType == typeof(Int64) || fields.FieldType == typeof(int))
                {
                    Int32 fieldNameLength = fields.Name.IndexOf(">") - (fields.Name.IndexOf("<") + 1);
                    string fieldName = fields.Name.Substring(fields.Name.IndexOf("<") + 1, fieldNameLength);
                    object fieldValue = fields.GetValue(objectClass);
                    if (!fieldName.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                    {
                        parametersValues.Add(fieldValue);
                        if ((count < objClassFields.Length) && (objClassFields.Length - count) == 1)
                        {
                            parameters.Add("@" + fieldName);
                            query += fieldName + "=";
                            complement = "@" + fieldName;
                        }
                        else
                        {
                            parameters.Add("@" + fieldName + ",");
                            query += fieldName + "=";
                            complement = "@" + fieldName + ",";
                        }

                        query += complement;
                    }
                }

                if (fields.FieldType == typeof(string))
                {
                    Int32 fieldNameLength = fields.Name.IndexOf(">") - (fields.Name.IndexOf("<") + 1);
                    string fieldName = fields.Name.Substring(fields.Name.IndexOf("<") + 1, fieldNameLength);
                    var fieldValue = fields.GetValue(objectClass);
                    parametersValues.Add(fieldValue);

                    if ((count < objClassFields.Length) && (objClassFields.Length - count) == 1)
                    {
                        parameters.Add("@" + fieldName);
                        query += fieldName + "=";
                        complement = "@" + fieldName;
                    }
                    else
                    {
                        parameters.Add("@" + fieldName + ",");
                        query += fieldName + "=";
                        complement = "@" + fieldName + ",";
                    }

                    query += complement;
                }

            }

            fields = objClassFields[0];

            Int32 fieldNameLengthZ = fields.Name.IndexOf(">") - (fields.Name.IndexOf("<") + 1);
            string fieldNameZ = fields.Name.Substring(fields.Name.IndexOf("<") + 1, fieldNameLengthZ);
            var fieldValueZ = fields.GetValue(objectClass);
            query += " WHERE 1 = 1 AND " + fieldNameZ + "=" + fieldValueZ;

            try
            {
                using (var context = new SqlConnection(_connectionString))
                {
                    context.Open();

                    SqlCommand sqlCommand = new SqlCommand(query, context);
                    for (int count = 0; count < parameters.Count; count++)
                    {
                        sqlCommand.Parameters.AddWithValue(parameters.ElementAt(count).ToString(), parametersValues.ElementAt(count).ToString());//objectClass.GetType().GetFields().ElementAt(count).GetValue(objectClass.GetType().GetFields()));
                    }
                    resultado = sqlCommand.ExecuteNonQuery();
                    context.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (resultado >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Executa um delete através dos dados em uma class(Utiliza o ID)
        /// </summary>
        /// <param name="objectClass">Classe</param>
        /// <returns>bool</returns>
        /// <rewards>
        /// Criado por: Luiz Lemos
        /// Criado em: 9/10/2017
        /// </rewards>
        public bool Delete(T objectClass)
        {
            string Table = string.Empty;
            BindingFlags flags = new BindingFlags();
            flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var objClassFields = objectClass.GetType().GetFields(flags);
            List<string> parameters = new List<string>();
            List<object> parametersValues = new List<object>();
            string complement = string.Empty;
            string name = "dbo." + objectClass.GetType().Name;
            Table = name;
            string query = "DELETE FROM " + Table + " WHERE ";

            for (int count = 0; count < objClassFields.Length; count++)
            {
                FieldInfo fields = objClassFields[count];
                if (fields.FieldType == typeof(Int64) || fields.FieldType == typeof(int))
                {
                    Int32 fieldNameLength = fields.Name.IndexOf(">") - (fields.Name.IndexOf("<") + 1);
                    string fieldName = fields.Name.Substring(fields.Name.IndexOf("<") + 1, fieldNameLength);
                    object fieldValue = fields.GetValue(objectClass);
                    if (fieldName.Equals("id", StringComparison.InvariantCultureIgnoreCase) && fields.GetValue(objectClass).ToString() != "0")
                    {
                        parametersValues.Add(fieldValue);
                        parameters.Add("@" + fieldName);
                        query += fieldName + "=";
                        complement = "@" + fieldName;

                        query += complement;
                    }
                }
            }

            try
            {
                using (var context = new SqlConnection(_connectionString))
                {
                    context.Open();

                    SqlCommand sqlCommand = new SqlCommand(query, context);
                    for (int count = 0; count < parameters.Count; count++)
                    {
                        sqlCommand.Parameters.AddWithValue(parameters.ElementAt(count).ToString(), parametersValues.ElementAt(count).ToString());//objectClass.GetType().GetFields().ElementAt(count).GetValue(objectClass.GetType().GetFields()));
                    }
                    resultado = sqlCommand.ExecuteNonQuery();
                    context.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (resultado >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion CRUD
    }
}
