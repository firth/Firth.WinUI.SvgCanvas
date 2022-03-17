using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Firth.WinUI
{
    public sealed class SignatureCanvas : Canvas
    {
        private bool Pressed = false;

        public SignatureCanvas()
        {
            PointerMoved += Canvas_PointerMoved;
            PointerPressed += Canvas_PointerPressed;
            
            PointerExited += PointerUp;
            PointerReleased += PointerUp;
            PointerCaptureLost += PointerUp;
            PointerCanceled += PointerUp;

            Background = new SolidColorBrush(Colors.White);
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var poly = new Polyline()
            {
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2
            };
            
            Children.Add(poly);
            Pressed = true;
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Children.LastOrDefault() != null && Pressed)
            {
                var p = e.GetCurrentPoint(sender as Canvas).Position;
                (Children.Last() as Polyline).Points.Add(new Point(p.X, p.Y));
            }
        }

        private void PointerUp(object sender, PointerRoutedEventArgs e)
        {
            Pressed = false;
        }

        public void Clear()
        {
            Children.Clear();
            Pressed = false;
        }

        public override string ToString()
        {
            // calculates the min/max for x and y, so that we can product an SVG where top left corner is (0,0)
            double minx = Children.SelectMany(x => (x as Polyline).Points.Select(y => y.X)).Min();
            double maxx = Children.SelectMany(x => (x as Polyline).Points.Select(y => y.X)).Max();

            double miny = Children.SelectMany(x => (x as Polyline).Points.Select(y => y.Y)).Min();
            double maxy = Children.SelectMany(x => (x as Polyline).Points.Select(y => y.Y)).Max();

            //starting with the SVG header that gets added to all SVGs
            string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"" + ((int)(maxx - minx)) + "\" height=\"" + ((int)(maxy - miny)) + "\" stroke=\"rgb(0, 0, 255)\">";
            foreach (Polyline stroke in Children)
            {
                svg += "<polyline stroke-linejoin=\"round\" stroke-linecap=\"round\" stroke-width=\"3\" stroke=\"rgb(0, 0, 255)\" fill=\"none\""
                    + " points=\"";

                int prev_x = -1, prev_y = -1;
                foreach(var p in stroke.Points)
                {
                    // filter out duplicates to keep the SVG optimized
                    if(((int)p.X) != prev_x || ((int)p.Y) != prev_y)
                    {
                        svg += ((int)(p.X - minx)) + "," + ((int)(p.Y - miny)) + " ";

                        prev_x = (int)p.X;
                        prev_y = (int)p.Y;
                    }
                }
                svg += "\"/>";
            }
            svg += "</svg>";
            return svg;
        }
    }
}
