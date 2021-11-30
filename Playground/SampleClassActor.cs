using Akka.Actor;


namespace Playground
{
    public partial class SampleClassActor : ReceiveActor
    {
        public static string GetString() => "a string";
    }
}