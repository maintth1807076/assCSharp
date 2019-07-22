using System;
using GiaoDichCSharp.entity;
using MySql.Data.MySqlClient;

namespace GiaoDichCSharp.model
{
    public class SHBAccountModel
    {
        public SHBAccount FindByUsernameAndPassword(string username, string password)
        {
            var cmd = new MySqlCommand("SELECT * FROM shbaccounts  WHERE username = @username and password = @password",ConnectionHelper.GetConnect());
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            var dataReader = cmd.ExecuteReader();
            SHBAccount shbAccount = null;
            if (dataReader.Read())
            {
                shbAccount = new SHBAccount
                {
                    AccountNumber = dataReader.GetString(0),
                    Username = dataReader.GetString(1),
                    Password = dataReader.GetString(2),
                    Balance = dataReader.GetDouble(3),
                };
            }
            ConnectionHelper.CloseConnect();
            return shbAccount;
        }
        public bool Save(SHBAccount shbAccount)
        {
            var cmd = new MySqlCommand("INSERT INTO shbaccounts (accountNumber, username, password, balance) values (@accountNumber, @username, @password, @balance)",ConnectionHelper.GetConnect());
            cmd.Parameters.AddWithValue("@accountNumber", shbAccount.AccountNumber);
            cmd.Parameters.AddWithValue("@username", shbAccount.Username);
            cmd.Parameters.AddWithValue("@password", shbAccount.Password);
            cmd.Parameters.AddWithValue("@balance", shbAccount.Balance);
            var result = cmd.ExecuteNonQuery();
            ConnectionHelper.CloseConnect();
            return result == 1;
        }
        public bool UpdateBalance(SHBAccount currentLoggedInAccount, SHBTransaction transaction)
        {
            ConnectionHelper.GetConnect();
            var trans = ConnectionHelper.GetConnect().BeginTransaction();
            try
            {
                var cmd = new MySqlCommand("SELECT balance FROM shbaccounts  WHERE accountNUmber = @accountNumber", ConnectionHelper.GetConnect());
                cmd.Parameters.AddWithValue("@accountNumber", currentLoggedInAccount.AccountNumber);
                var dataReader = cmd.ExecuteReader();
                double currentBalance = 0;
                if (dataReader.Read())
                {
                    currentBalance = dataReader.GetDouble("balance");
                }
                dataReader.Close();
                if (transaction.Type == SHBTransaction.TransactionType.WITHDRAW &&
                    currentBalance < transaction.Amount)
                {
                    throw new Exception("Không đủ tiền trong tài khoản.");
                }

                if (transaction.Type == SHBTransaction.TransactionType.WITHDRAW)
                {
                    currentBalance -= transaction.Amount;
                }
                if (transaction.Type == SHBTransaction.TransactionType.DEPOSIT)
                {
                    currentBalance += transaction.Amount;
                }
                var cmd1 = new MySqlCommand("UPDATE shbaccounts SET balance = @balance WHERE accountNumber = @accountNumber", ConnectionHelper.GetConnect());
                cmd1.Parameters.AddWithValue("@balance", currentBalance);
                cmd1.Parameters.AddWithValue("@accountNumber", currentLoggedInAccount.AccountNumber);
                var updateResult = cmd1.ExecuteNonQuery();
                var cmd2 = new MySqlCommand("INSERT INTO shbtransactions (transactionId, senderAccountNumber, receiverAccountNumber, type, amount, message, createdAt, updatedAt, status) values ( @transactionId, @senderAccountNumber, @receiverAccountNumber, @type, @amount, @message, @createdAt, @updatedAt, @status) ", ConnectionHelper.GetConnect());
                cmd2.Parameters.AddWithValue("@transactionId", transaction.TransactionId);
                cmd2.Parameters.AddWithValue("@senderAccountNumber", transaction.SenderAccountNumber);
                cmd2.Parameters.AddWithValue("@receiverAccountNumber", transaction.ReceiverAccountNumber);
                cmd2.Parameters.AddWithValue("@type", transaction.Type);
                cmd2.Parameters.AddWithValue("@amount", transaction.Amount);
                cmd2.Parameters.AddWithValue("@message", transaction.Message);
                cmd2.Parameters.AddWithValue("@createdAt", transaction.CreatedAtMLS);
                cmd2.Parameters.AddWithValue("@updatedAt", transaction.UpdatedAtMLS);
                cmd2.Parameters.AddWithValue("@status", transaction.Status);
                var transactionResult = cmd2.ExecuteNonQuery();
                if (updateResult != 1 || transactionResult != 1)
                {
                    throw new Exception("Không thể thêm giao dịch hoặc update tài khoản.");
                }
                trans.Commit();
            }
            catch (Exception e)
            {
                trans.Rollback();
                Console.WriteLine(e);
                return false;
            }
            ConnectionHelper.CloseConnect();
            return true;
        }
        
        public bool Transfer(SHBAccount currentLoggedInAccount, SHBTransaction transaction)
        {
            ConnectionHelper.GetConnect();
            var trans = ConnectionHelper.GetConnect().BeginTransaction();
            try
            {
                var cmd = new MySqlCommand("SELECT balance FROM shbaccounts  WHERE accountNUmber = @accountNumber", ConnectionHelper.GetConnect());
                cmd.Parameters.AddWithValue("@accountNumber", currentLoggedInAccount.AccountNumber);
                var dataReader = cmd.ExecuteReader();
                double currentBalance = 0;
                if (dataReader.Read())
                {
                    currentBalance = dataReader.GetDouble("balance");
                }
                dataReader.Close();
                if (currentBalance < transaction.Amount)
                {
                    throw new Exception("Không đủ tiền trong tài khoản.");
                }
                currentBalance -= transaction.Amount;
                
                var cmd1 = new MySqlCommand("UPDATE shbaccounts SET balance = @balance WHERE accountNumber = @accountNumber", ConnectionHelper.GetConnect());
                cmd1.Parameters.AddWithValue("@balance", currentBalance);
                cmd1.Parameters.AddWithValue("@accountNumber", currentLoggedInAccount.AccountNumber);
                var updateResult = cmd1.ExecuteNonQuery();

                var cmd2 = new MySqlCommand("SELECT balance FROM shbaccounts  WHERE accountNUmber = @accountNumber", ConnectionHelper.GetConnect());
                cmd2.Parameters.AddWithValue("@accountNumber",transaction.ReceiverAccountNumber);
                var dataReader1 = cmd2.ExecuteReader();
                double receiverBalance = 0;
                if (dataReader1.Read())
                {
                    receiverBalance = dataReader1.GetDouble("balance");
                }
                dataReader1.Close();              
                receiverBalance += transaction.Amount;
                
                var cmd3 = new MySqlCommand("UPDATE shbaccounts SET balance = @balance WHERE accountNumber = @accountNumber", ConnectionHelper.GetConnect());
                cmd3.Parameters.AddWithValue("@balance", receiverBalance);
                cmd3.Parameters.AddWithValue("@accountNumber", transaction.ReceiverAccountNumber);
                var updateResultReceiver = cmd3.ExecuteNonQuery();

                var cmd4 = new MySqlCommand("INSERT INTO shbtransactions (transactionId, senderAccountNumber, receiverAccountNumber, type, amount, message, createdAt, updatedAt, status) values ( @transactionId, @senderAccountNumber, @receiverAccountNumber, @type, @amount, @message, @createdAt, @updatedAt, @status) ", ConnectionHelper.GetConnect());
                cmd4.Parameters.AddWithValue("@transactionId", transaction.TransactionId);
                cmd4.Parameters.AddWithValue("@senderAccountNumber", transaction.SenderAccountNumber);
                cmd4.Parameters.AddWithValue("@receiverAccountNumber", transaction.ReceiverAccountNumber);
                cmd4.Parameters.AddWithValue("@type", transaction.Type);
                cmd4.Parameters.AddWithValue("@amount", transaction.Amount);
                cmd4.Parameters.AddWithValue("@message", transaction.Message);
                cmd4.Parameters.AddWithValue("@createdAt", transaction.CreatedAtMLS);
                cmd4.Parameters.AddWithValue("@updatedAt", transaction.UpdatedAtMLS);
                cmd4.Parameters.AddWithValue("@status", transaction.Status);
                var transactionResult = cmd4.ExecuteNonQuery();

                if (updateResult != 1 || transactionResult != 1 || updateResultReceiver != 1)
                {
                    throw new Exception("Không thể thêm giao dịch hoặc update tài khoản.");
                }

                trans.Commit();
                return true;
            }
            catch (Exception e)
            {
                trans.Rollback();
                return false;
            }
            finally
            {                
               ConnectionHelper.CloseConnect();
            }
        }

        public SHBAccount FindByAccountNumber(string accountNumber)
        {
            var cmd = new MySqlCommand("SELECT * FROM shbaccounts  WHERE accountNumber = @accountNumber",ConnectionHelper.GetConnect());
            cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            var dataReader = cmd.ExecuteReader();
            SHBAccount shbAccount = null;
            if (dataReader.Read())
            {
                shbAccount = new SHBAccount
                {
                    AccountNumber = dataReader.GetString(0),
                    Username = dataReader.GetString(1),
                    Password = dataReader.GetString(2),
                    Balance = dataReader.GetDouble(3),
                };
            }
            ConnectionHelper.CloseConnect();
            return shbAccount;
        }
    }
}