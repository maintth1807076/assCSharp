using System;
using GiaoDichCSharp.entity;
using MySql.Data.MySqlClient;

namespace GiaoDichCSharp.model
{
    public class BlockchainAddressModel
    {
        public BlockchainAddress FindByAddressAndPrivateKey(string address, string privateKey)
        {
            var cmd = new MySqlCommand("SELECT * FROM blockchainaccounts  WHERE address = @address and privateKey = @privateKey",ConnectionHelper.GetConnect());
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@privateKey", privateKey);
            var dataReader = cmd.ExecuteReader();
            BlockchainAddress blockchainAddress = null;
            if (dataReader.Read())
            {
                blockchainAddress = new BlockchainAddress
                {
                    Address = dataReader.GetString(0),
                    PrivateKey = dataReader.GetString(1),
                    Balance = dataReader.GetDouble(2),
                };
            }
            ConnectionHelper.CloseConnect();
            return blockchainAddress;
        }
        public bool Save(BlockchainAddress blockchainAddress)
        {
            var cmd = new MySqlCommand("INSERT INTO blockchainaccounts (address, privateKey, balance) values (@address, @privateKey, @balance)",ConnectionHelper.GetConnect());
            cmd.Parameters.AddWithValue("@address", blockchainAddress.Address);
            cmd.Parameters.AddWithValue("@privateKey", blockchainAddress.PrivateKey);
            cmd.Parameters.AddWithValue("@balance", blockchainAddress.Balance);
            var result = cmd.ExecuteNonQuery();
            ConnectionHelper.CloseConnect();
            return result == 1;
        }
        public bool UpdateBalance(BlockchainAddress currentLoggedInAddress, BlockchainTransaction transaction)
        {
            ConnectionHelper.GetConnect();
            var trans = ConnectionHelper.GetConnect().BeginTransaction();
            try
            {
                var cmd = new MySqlCommand("SELECT balance FROM blockchainaccounts  WHERE address = @address", ConnectionHelper.GetConnect());
                cmd.Parameters.AddWithValue("@address", currentLoggedInAddress.Address);
                var dataReader = cmd.ExecuteReader();
                double currentBalance = 0;
                if (dataReader.Read())
                {
                    currentBalance = dataReader.GetDouble("balance");
                }
                dataReader.Close();
                if (transaction.Type == BlockchainTransaction.TransactionType.WITHDRAW &&
                    currentBalance < transaction.Amount)
                {
                    throw new Exception("Không đủ tiền trong tài khoản.");
                }

                if (transaction.Type == BlockchainTransaction.TransactionType.WITHDRAW)
                {
                    currentBalance -= transaction.Amount;
                }
                if (transaction.Type == BlockchainTransaction.TransactionType.DEPOSIT)
                {
                    currentBalance += transaction.Amount;
                }
                var cmd1 = new MySqlCommand("UPDATE blockchainaccounts SET balance = @balance WHERE address = @address", ConnectionHelper.GetConnect());
                cmd1.Parameters.AddWithValue("@balance", currentBalance);
                cmd1.Parameters.AddWithValue("@address", currentLoggedInAddress.Address);
                var updateResult = cmd1.ExecuteNonQuery();
                var cmd2 = new MySqlCommand("INSERT INTO blockchaintransactions (transactionId, senderAddress, receiverAddress, type, amount, createdAt, updatedAt, status) values ( @transactionId, @senderAddress, @receiverAddress, @type, @amount, @createdAt, @updatedAt, @status) ", ConnectionHelper.GetConnect());
                cmd2.Parameters.AddWithValue("@transactionId", transaction.TransactionId);
                cmd2.Parameters.AddWithValue("@senderAddress", transaction.SenderAddress);
                cmd2.Parameters.AddWithValue("@receiverAddress", transaction.ReceiverAddress);
                cmd2.Parameters.AddWithValue("@type", transaction.Type);
                cmd2.Parameters.AddWithValue("@amount", transaction.Amount);
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
        
        public bool Transfer(BlockchainAddress currentLoggedInAddress, BlockchainTransaction transaction)
        {
            ConnectionHelper.GetConnect();
            var trans = ConnectionHelper.GetConnect().BeginTransaction();
            try
            {
                var cmd = new MySqlCommand("SELECT balance FROM blockchainaccounts  WHERE address = @address", ConnectionHelper.GetConnect());
                cmd.Parameters.AddWithValue("@address", currentLoggedInAddress.Address);
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
                
                var cmd1 = new MySqlCommand("UPDATE blockchainaccounts SET balance = @balance WHERE address = @address", ConnectionHelper.GetConnect());
                cmd1.Parameters.AddWithValue("@balance", currentBalance);
                cmd1.Parameters.AddWithValue("@address", currentLoggedInAddress.Address);
                var updateResult = cmd1.ExecuteNonQuery();

                var cmd2 = new MySqlCommand("SELECT balance FROM blockchainaccounts  WHERE address = @address", ConnectionHelper.GetConnect());
                cmd2.Parameters.AddWithValue("@address",transaction.ReceiverAddress);
                var dataReader1 = cmd2.ExecuteReader();
                double receiverBalance = 0;
                if (dataReader1.Read())
                {
                    receiverBalance = dataReader1.GetDouble("balance");
                }
                dataReader1.Close();              
                receiverBalance += transaction.Amount;
                
                var cmd3 = new MySqlCommand("UPDATE blockchainaccounts SET balance = @balance WHERE address = @address", ConnectionHelper.GetConnect());
                cmd3.Parameters.AddWithValue("@balance", receiverBalance);
                cmd3.Parameters.AddWithValue("@address", transaction.ReceiverAddress);
                var updateResultReceiver = cmd3.ExecuteNonQuery();

                var cmd4 = new MySqlCommand("INSERT INTO blockchaintransactions (transactionId, senderAddress, receiverAddress, type, amount, createdAt, updatedAt, status) values ( @transactionId, @senderAddress, @receiverAddress, @type, @amount, @createdAt, @updatedAt, @status) ", ConnectionHelper.GetConnect());
                cmd4.Parameters.AddWithValue("@transactionId", transaction.TransactionId);
                cmd4.Parameters.AddWithValue("@senderAddress", transaction.SenderAddress);
                cmd4.Parameters.AddWithValue("@receiverAddress", transaction.ReceiverAddress);
                cmd4.Parameters.AddWithValue("@type", transaction.Type);
                cmd4.Parameters.AddWithValue("@amount", transaction.Amount);
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

        public BlockchainAddress FindByAddress(string address)
        {
            var cmd = new MySqlCommand("SELECT * FROM blockchainaccounts  WHERE address = @address",ConnectionHelper.GetConnect());
            cmd.Parameters.AddWithValue("@address", address);
            var dataReader = cmd.ExecuteReader();
            BlockchainAddress blockchainAddress = null;
            if (dataReader.Read())
            {
                blockchainAddress = new BlockchainAddress
                {
                    Address = dataReader.GetString(0),
                    PrivateKey = dataReader.GetString(1),
                    Balance = dataReader.GetDouble(2),
                };
            }
            ConnectionHelper.CloseConnect();
            return blockchainAddress;
        }
    }
}