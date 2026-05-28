namespace Exam.Application.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException()
            : base("Requested item was not found.")
        {
        }

        public ItemNotFoundException(string message)
            : base(message)
        {
        }
    }
}
