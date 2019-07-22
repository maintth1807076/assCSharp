using System;
using GiaoDichCSharp.entity;
using GiaoDichCSharp.model;

namespace GiaoDichCSharp.demo
{
    public class GiaoDichSHB : GiaoDich
    {
        private static SHBAccountModel model;
        
        public GiaoDichSHB()
        {
           model = new SHBAccountModel(); 
        }
        public void RutTien()
        {
            Console.WriteLine("Nhập số tiền muốn rút:");
            var amount = double.Parse(Console.ReadLine());
            if (amount < 0)
            {
                Console.WriteLine("Số lượng không hợp lí.");
                return;
            }
            Program.currentLoggedInAccount = model.FindByAccountNumber(Program.currentLoggedInAccount.AccountNumber);
            if (Program.currentLoggedInAccount.Balance < amount)
            {
                Console.WriteLine("Không đủ tiền.");
                return;
            }
            var transaction = new SHBTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                SenderAccountNumber = Program.currentLoggedInAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInAccount.AccountNumber,
                Type = SHBTransaction.TransactionType.WITHDRAW,
                Amount = amount,
                Message = "Rút tiền: " + amount,
                CreatedAtMLS = DateTime.Now.Ticks,
                UpdatedAtMLS = DateTime.Now.Ticks,
                Status = SHBTransaction.TransactionStatus.COMPLETED
            };
            if (model.UpdateBalance(Program.currentLoggedInAccount, transaction))
            {
                Console.WriteLine("Giao dịch thành công.");
            }
        }

        public void GuiTien()
        {
            Console.WriteLine("Nhập số tiền muốn gửi:");
            var amount = double.Parse(Console.ReadLine());
            if (amount < 0)
            {
                Console.WriteLine("Số lượng không hợp lí.");
                return;
            }
            var transaction = new SHBTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                SenderAccountNumber = Program.currentLoggedInAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInAccount.AccountNumber,
                Type = SHBTransaction.TransactionType.DEPOSIT,
                Amount = amount,
                Message = "Gửi tiền: " + amount,
                CreatedAtMLS = DateTime.Now.Ticks,
                UpdatedAtMLS = DateTime.Now.Ticks,
                Status = SHBTransaction.TransactionStatus.COMPLETED
            };
            if (model.UpdateBalance(Program.currentLoggedInAccount, transaction))
            {
                Console.WriteLine("Giao dịch thành công.");
            }
        }

        public void ChuyenKhoan()
        {
            Console.WriteLine("Vui lòng nhập số tài khoản chuyển tiền: ");
            var accountNumber = Console.ReadLine();
            var receiverAccount = model.FindByAccountNumber(accountNumber);
            if (receiverAccount == null)
            {
                Console.WriteLine("Tài khoản nhận tiền không tồn tại hoặc đã bị khoá.");
                return;
            }
            Console.WriteLine("Tài khoản nhận tiền: " + accountNumber);
            Console.WriteLine("Chủ tài khoản: " + receiverAccount.Username + receiverAccount.Balance);
            Console.WriteLine("Nhập số tiền chuyển khoản: ");
            var amount = double.Parse(Console.ReadLine());;
            if (amount < 0)
            {
                Console.WriteLine("Số lượng không hợp lí.");
                return;
            }
            Program.currentLoggedInAccount = model.FindByAccountNumber(Program.currentLoggedInAccount.AccountNumber);
            if (amount > Program.currentLoggedInAccount.Balance)
            {
                Console.WriteLine("Số dư tài khoản không đủ thực hiện giao dịch.");
                return;
            }
            var transaction = new SHBTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                SenderAccountNumber = Program.currentLoggedInAccount.AccountNumber,
                ReceiverAccountNumber = accountNumber,
                Type = SHBTransaction.TransactionType.TRANSFER,
                Amount = amount,
                Message = $"{Program.currentLoggedInAccount.AccountNumber} chuyển tới {accountNumber}: {amount}",
                CreatedAtMLS = DateTime.Now.Ticks,
                UpdatedAtMLS = DateTime.Now.Ticks,
                Status = SHBTransaction.TransactionStatus.COMPLETED
            };
            if (model.Transfer(Program.currentLoggedInAccount, transaction))
            {
                Console.WriteLine("Giao dịch thành công.");
            }
            else
            {
                Console.WriteLine("Giao dịch thất bại, vui lòng thử lại.");
            }
        }
        
        public void Login()
        {
            Console.Clear();
            Console.WriteLine("Đăng nhập với tài khoản SHB.");
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            var account = model.FindByUsernameAndPassword(username, password);
            if (account == null)
            {
                Console.WriteLine("Đăng nhập thất bại.");
                return;
            }

            Program.currentLoggedInAccount = new SHBAccount
            {
                AccountNumber = account.AccountNumber,
                Username = account.Username,
                Password = account.Password,
                Balance = account.Balance
            };

        }

        public void Register()
        {
            model.Save(new SHBAccount()
            {
                AccountNumber = "A03",
                Username = "HaHa",
                Password = "123",
                Balance = 0
            });
        }
    }
}