using System.Windows;
using System.Windows.Documents;

namespace InperStudioControlLib.Control.InperAdorner
{
    public class InperAdornerLayer
    {
        public static void Show(UIElement uIElement)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(uIElement);
            if (layer != null)
            {
                layer.Add(new InperCustomAdorner(uIElement));
            }
        }
        public static void Close(UIElement uIElement)
        {
            var layer = AdornerLayer.GetAdornerLayer(uIElement);
            var arr = layer.GetAdorners(uIElement);
            if (arr != null)
            {
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    layer.Remove(arr[i]);
                }
            }
        }
    }
}
