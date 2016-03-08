namespace ProtoZBuffer.Generators
{
    interface IGenerator
    {
        ///<summary>Entry point</summary> 
        bool Launch();

        ///<summary>Path to the input protoz file</summary> 
        string ProtoZFile { get; set; }

        ///<summary>Namespace in which the generated code will be put</summary> 
        string Namespace { get; set; }

        ///<summary>Where to store the generated code</summary> 
        string OutputFolder { get; set; }
    }
}
