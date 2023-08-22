using System;
using System.Windows;
using System.Windows.Input;

namespace GriseoRenderer.Foundation;

public class InputManager
{
    public Action OnW;
    public Action OnA;
    public Action OnS;
    public Action OnD;
    public Action OnQ;
    public Action OnE;

    public void Query()
    {
        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            if (Keyboard.IsKeyDown(Key.W)) OnW?.Invoke();
            if (Keyboard.IsKeyDown(Key.A)) OnA?.Invoke();
            if (Keyboard.IsKeyDown(Key.S)) OnS?.Invoke();
            if (Keyboard.IsKeyDown(Key.D)) OnD?.Invoke();
            if (Keyboard.IsKeyDown(Key.Q)) OnQ?.Invoke();
            if (Keyboard.IsKeyDown(Key.E)) OnE?.Invoke();
        }));
    }
}