namespace Achiever.Common.Model
{
    public class AchieverContextHolder
    {
        static AchieverContext Context = null;
        public static AchieverContext GetContext()
        {
            /*if (Context == null)
            {
                Context = new AchieverContext();
            }
            return Context;*/
            return new AchieverContext();
        }
    }
}
