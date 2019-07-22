namespace GiaoDichCSharp.entity
{
    public class SHBAccount
    {
        public string AccountNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public double Balance { get; set; }

        public SHBAccount()
        {
        }

        public override string ToString()
        {
            return $"Username: {Username} || Balance: {Balance}";
        }
    }
}