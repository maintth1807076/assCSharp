namespace GiaoDichCSharp.entity
{
    public class SHBTransaction
    {
        public string TransactionId { get; set; }
        public string SenderAccountNumber { get; set; }
        public string ReceiverAccountNumber { get; set; }
        public  TransactionType Type { get; set; }
        public double Amount { get; set; }
        public string Message { get; set; }
        public long CreatedAtMLS { get; set; }
        public long UpdatedAtMLS { get; set; }
        public TransactionStatus Status { get; set; }
        
        public enum TransactionStatus
        {
            COMPLETED = 1,
            DELETED = -1,
            PENDING = 0
        } 
        
        public enum TransactionType
        {
            WITHDRAW = 1,
            DEPOSIT = 2,
            TRANSFER = 3
        }
    }
}