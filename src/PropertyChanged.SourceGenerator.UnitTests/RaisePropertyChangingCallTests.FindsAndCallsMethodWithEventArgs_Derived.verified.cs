﻿partial class Derived
{
    public string Foo
    {
        get => this._foo;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this._foo))
            {
                this.OnPropertyChanging(global::PropertyChanged.SourceGenerator.Internal.EventArgsCache.PropertyChanging_Foo);
                this._foo = value;
                this.OnPropertyChanged(global::PropertyChanged.SourceGenerator.Internal.EventArgsCache.PropertyChanged_Foo);
            }
        }
    }
}