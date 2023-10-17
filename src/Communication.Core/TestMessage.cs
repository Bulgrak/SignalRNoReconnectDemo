using System;

namespace Communication.Core
{
    [Serializable]
    public class TestMessage
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}