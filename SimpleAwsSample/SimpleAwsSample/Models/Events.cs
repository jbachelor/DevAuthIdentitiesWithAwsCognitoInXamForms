using System;
using Prism.Events;

namespace SimpleAwsSample.Models
{
    public class AddTextToUiOutput : PubSubEvent<string> { }
}
