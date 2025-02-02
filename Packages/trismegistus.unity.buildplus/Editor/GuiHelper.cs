namespace Trismegistus.BuildPlus
{
    public class GuiColorScope : System.IDisposable
    {
        public GuiColorScope(UnityEngine.Color color)
        {
            UnityEngine.GUI.color = color;
        }

        public void Dispose()
        {
            UnityEngine.GUI.color = UnityEngine.Color.white;
        }
    }
}