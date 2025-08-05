using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RunNow.Models;

namespace RunNow.Services
{
    public class GenericMessage<T> : ValueChangedMessage<T>
    {
        public GenericMessage(T value) : base(value) { }
    }
    public static class MessengerService
    {
        // ✅ 메시지 전송
        public static void Send<T>(T value)
        {
            WeakReferenceMessenger.Default.Send(new GenericMessage<T>(value));
        }

        // ✅ 메시지 수신자 등록
        public static void Register<T>(object recipient, Action<T> onReceived)
        {
            WeakReferenceMessenger.Default.Register<GenericMessage<T>>(recipient, (r, m) =>
            {
                onReceived?.Invoke(m.Value);
            });
        }

        // ❌ 수신 해제도 필요할 경우
        public static void Unregister<T>(object recipient)
        {
            WeakReferenceMessenger.Default.Unregister<GenericMessage<T>>(recipient);
        }
    }
}
