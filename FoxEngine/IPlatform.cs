﻿// Copyright (c) 2019 FoxCouncil - License: MIT
// https://github.com/FoxCouncil/CSCE

using System;

namespace FoxEngine
{
    public interface IPlatform
    {
        event Action<int> OnKeyUp;

        void MessageBox(string title, string message);

        void Initialize();

        void Run();

        void Dispose();

        void CreateGlContext();

        void Draw();

        void SetWindowTitle(string title);
    }
}