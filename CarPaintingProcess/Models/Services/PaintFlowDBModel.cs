using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Common;
using Prism.Mvvm;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows;

namespace CarPaintingProcess.Models.Services
{
    public class DBlink : BindableBase
    {
        private string _server = "hy.shogle.net"; // _server : ip 주소
        private string _port = "3306"; // _port : 포트번호
        private string _dbName = "PaintFlowDB"; // _dbName : 연결 스키마
        private string _dbId = "root"; // _dbId : 접속아이디
        private string _dbPw = "9671"; // _dbpw : 접속패스워드

        static DBlink staticDBlink; // DB 연결 객체 생성
        MySqlConnection connection; // DB connection 객체

        private DBlink() { } // 생성자 접근 제어 변경
        public static DBlink Instance()
        {
            if (staticDBlink == null)
            {
                staticDBlink = new DBlink();
            }
            return staticDBlink;
        }
        // -----------
        //private void setDBLink()
        //{
        //    string relativePath = @".\Models\DB.txt";
        //    string fullPath = Path.Combine(Environment.CurrentDirectory, relativePath);
        //    StreamReader sr = new StreamReader(fullPath);
        //    string[] Data = sr.ReadToEnd().Split("\n");
        //    _server = Data[0];
        //    _port = Data[1];
        //    _dbName = Data[2];
        //    _dbId = Data[3];
        //    _dbPw = Data[4];
        //}
        public void Connect()
        {
            //this.setDBLink();
            string myConnection = "Server=" + _server + ";Port=" + _port + ";Database=" + _dbName + ";User Id = " + _dbId + ";Password = " + _dbPw + ";CharSet=utf8;";
            try
            {
                connection = new MySqlConnection(myConnection);
                connection.Open(); // DB 오픈
            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString());
            }
        }
        // DB 연결 확인 함수
        public bool ConnectOk()
        {
            try
            {
                if (connection.Ping())
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        // DB insert
        public bool Insert(string sql)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, this.connection); // sql 실행
                if (cmd.ExecuteNonQuery() == 1) // 정상 수행 완료
                    return true;
                else
                    return false;
            }
            catch
            {
                return false; // db insert 에러
            }
        }

        public void Update(string sql)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, this.connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("데이터가 반영되지 않았습니다!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public List<Dictionary<string, object>> Select(string sql)
        {
            // 간단한 Select문 메소드, 불러오는 데이터가 크면 그냥 직접 Select을 하는 것을 추천
            // 결과 저장 List
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            // 데이터 필드에 따른 길이
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.GetValue(i);
                                row[columnName] = value;
                            }
                            resultList.Add(row);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("데이터베이스 접속 오류", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return resultList;
        }

        public void Disconnect()
        {
            connection.Close(); // 연결 해제
        }
    }
}
