using System.Text.Json;

namespace SCLibs.StateContainer
{
    public abstract class BaseClass
    {
        public override string ToString()
        {
            try
            {
                return JsonSerializer.Serialize(this);
            }
            catch (Exception)
            {
                // In case serialization fails, fall back to base implementation
                return base.ToString() ?? string.Empty;
            }
        }
    }
}