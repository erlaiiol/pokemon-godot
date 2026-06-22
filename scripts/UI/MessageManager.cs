using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using pokemonGodot.Scripts.Core;

namespace pokemonGodot.Scripts.UI
{

    public partial class MessageManager : CanvasLayer
    {
        public static MessageManager Instance { get; private set; }

        [ExportCategory("Components")]
        [Export] public NinePatchRect Box;

        [Export] public RichTextLabel Label;

        [ExportCategory("Variables")]
        [Export] public bool IsScrolling = false;
        [Export] public int Delay = 15;

        [Export] public Array<string> Messages;

        public override void _Ready()
        {
            Instance = this;
        }

        public static void PlayText(params string[] payload)
        {
            if (IsReading()) return;
            if (payload.Length == 0) return;

            //TODO: Signal Message Open
            Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, true);

            Instance.Messages = [..payload];
            ScrollText();

        }

        public static async void ScrollText()
        {
            if (!IsReading())
            {
                Instance.Box.Visible = true;
            }

            if (Instance.Messages.Count == 0)
            {
                Instance.Box.Visible = false;
                return;
            }

            Instance.IsScrolling = true;
            Instance.Label.Text = "";

            foreach (char c in Instance.Messages[0])
            {
                Instance.Label.Text += c;
                await Task.Delay(Instance.Delay);
            }

            Instance.Messages.RemoveAt(0);
            Instance.IsScrolling = false;    
        }

        public static bool IsReading()
        {
            return Instance.Box.Visible;
        }

        public static bool Scrolling()
        {
            return Instance.IsScrolling;
        }

        public static Array<string> GetMessages()
        {
            return Instance.Messages;
        }


    }


}
