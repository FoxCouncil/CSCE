// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    public class Button
    {
        public bool Pressed;
        public bool Released;
        public bool Held;

        public override string ToString()
        {
            return $"Button({Pressed}, {Released}, {Held})";
        }
    }
}
