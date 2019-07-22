using System;
using GiaoDichCSharp.entity;
using GiaoDichCSharp.model;

namespace GiaoDichCSharp.demo
{
    public class GiaoDichBlockChain : GiaoDich
    {
        private static BlockchainAddressModel model;
        
        public GiaoDichBlockChain()
        {
           model = new BlockchainAddressModel(); 
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
            Program.currentLoggedInAddress = model.FindByAddress(Program.currentLoggedInAddress.Address);
            if (Program.currentLoggedInAddress.Balance < amount)
            {
                Console.WriteLine("Không đủ tiền.");
                return;
            }
            var transaction = new BlockchainTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                SenderAddress = Program.currentLoggedInAddress.Address,
                ReceiverAddress = Program.currentLoggedInAddress.Address,
                Type = BlockchainTransaction.TransactionType.WITHDRAW,
                Amount = amount,
                CreatedAtMLS = DateTime.Now.Ticks,
                UpdatedAtMLS = DateTime.Now.Ticks,
                Status = BlockchainTransaction.TransactionStatus.COMPLETED
            };
            if (model.UpdateBalance(Program.currentLoggedInAddress, transaction))
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
            var transaction = new BlockchainTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                SenderAddress = Program.currentLoggedInAddress.Address,
                ReceiverAddress = Program.currentLoggedInAddress.Address,
                Type = BlockchainTransaction.TransactionType.DEPOSIT,
                Amount = amount,
                CreatedAtMLS = DateTime.Now.Ticks,
                UpdatedAtMLS = DateTime.Now.Ticks,
                Status = BlockchainTransaction.TransactionStatus.COMPLETED
            };
            if (model.UpdateBalance(Program.currentLoggedInAddress, transaction))
            {
                Console.WriteLine("Giao dịch thành công.");
            }
        }

        public void ChuyenKhoan()
        {
            Console.WriteLine("Vui lòng nhập số tài khoản chuyển tiền: ");
            var address = Console.ReadLine();
            var receiverAddress = model.FindByAddress(address);
            if (receiverAddress == null)
            {
                Console.WriteLine("Tài khoản nhận tiền không tồn tại hoặc đã bị khoá.");
                return;
            }
            Console.WriteLine("Tài khoản nhận tiền: " + address);
            Console.WriteLine("Nhập số tiền chuyển khoản: ");
            var amount = double.Parse(Console.ReadLine());;
            if (amount < 0)
            {
                Console.WriteLine("Số lượng không hợp lí.");
                return;
            }
            Program.currentLoggedInAddress = model.FindByAddress(Program.currentLoggedInAddress.Address);
            if (amount > Program.currentLoggedInAddress.Balance)
            {
                Console.WriteLine("Số dư tài khoản không đủ thực hiện giao dịch.");
                return;
            }
            var transaction = new BlockchainTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                SenderAddress = Program.currentLoggedInAddress.Address,
                ReceiverAddress = address,
                Type = BlockchainTransaction.TransactionType.TRANSFER,
                Amount = amount,
                CreatedAtMLS = DateTime.Now.Ticks,
                UpdatedAtMLS = DateTime.Now.Ticks,
                Status = BlockchainTransaction.TransactionStatus.COMPLETED
            };
            if (model.Transfer(Program.currentLoggedInAddress, transaction))
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
            Console.WriteLine("Đăng nhập với tài khoản Blockchain.");
            Console.WriteLine("Address: ");
            var address = Console.ReadLine();
            Console.WriteLine("PrivateKey: ");
            var privateKey = Console.ReadLine();
            var blockchainAddress = model.FindByAddressAndPrivateKey(address, privateKey);
            if (blockchainAddress == null)
            {
                Console.WriteLine("Đăng nhập thất bại.");
                return;
            }

            Program.currentLoggedInAddress = new BlockchainAddress
            {
                Address = blockchainAddress.Address,
                PrivateKey = blockchainAddress.PrivateKey,
                Balance = blockchainAddress.Balance
            };

        }

        public void Register()
        {
            model.Save(new BlockchainAddress
            {
                Address = "789haha",
                PrivateKey = "123",
                Balance = 400000
            });
        }
    }
}