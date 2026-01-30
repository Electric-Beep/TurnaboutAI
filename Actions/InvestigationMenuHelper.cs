using System.Collections;
using TurnaboutAI.Utility;

namespace TurnaboutAI.Actions
{
    internal static class InvestigationMenuHelper
    {
        public static IEnumerator SelectOption(int index)
        {
            while (true)
            {
                int cursor = tanteiMenu.instance.cursor_no;

                if (cursor < index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Right, Waiter.ShortWait);
                }
                else if (cursor > index)
                {
                    yield return KeyPresser.PressAndWait(KeyType.Left, Waiter.ShortWait);
                }
                else
                {
                    break;
                }
            }

            yield return Waiter.Wait(Waiter.ShortWait);
            KeyPresser.PressKey(KeyType.A);
        }
    }
}
