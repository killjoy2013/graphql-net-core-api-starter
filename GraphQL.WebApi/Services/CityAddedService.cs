using GraphQL.WebApi.Dto;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GraphQL.WebApi.Services
{
    public class CityAddedService
    {
        private readonly ISubject<CityAddedMessage> _messageStream = new ReplaySubject<CityAddedMessage>(1);
        public CityAddedMessage AddCityAddedMessage(CityAddedMessage message)
        {
            _messageStream.OnNext(message);
            return message;
        }

        public IObservable<CityAddedMessage> GetMessages(string countryName)
        {
            var mess = _messageStream
                .Where(message =>
                    message.countryName == countryName
                ).Select(s => s)
                .AsObservable();

            return mess;
        }
    }
}
