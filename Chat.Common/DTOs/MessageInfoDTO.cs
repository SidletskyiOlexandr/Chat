namespace Chat.Common.DTOs
{
    public class MessageInfoDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string SenderId { get; set; }
        public int ChatId { get; set; }
        public int ChatType { get; set; }
    }
}
