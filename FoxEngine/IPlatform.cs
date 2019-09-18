// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

namespace FoxEngine
{
    public interface IPlatform
    {
        void MessageBox(string title, string message);

        void Initialize();

        void Run();

        void Dispose();

        void CreateGlContext();

        void Draw();

        void SetWindowTitle(string title);
    }
}