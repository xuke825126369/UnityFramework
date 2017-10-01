namespace xk_System.Model
{
    public class TimeMessage : NetModel
    {
        public DataBind<ulong> mDataBindServerTime=new DataBind<ulong>();

        public override void initModel()
        {
            base.initModel();
        }

        public override void destroyModel()
        {
            base.destroyModel();
        }
    }
}