using System;
using System.Collections.Generic;

namespace FlightNotificationSystem
{
    public enum FlightStatus
    {
        Scheduled,
        Delayed,
        GateChanged,
        BoardingStarted
    }

    public static class EventLogger
    {
        private static int _minute = 0;

        public static void Log(string source, string message)
        {
            string time = TimeSpan.FromMinutes(600 + _minute).ToString(@"hh\:mm");
            _minute += 5;
            Console.WriteLine($"[{time}] [{source}] {message}");
        }
    }

    public interface IFlightObserver
    {
        string Name { get; }
        void Update(string flightNumber, FlightStatus status, string details);
    }

    public interface IFlightSubject
    {
        void Subscribe(IFlightObserver observer);
        void Unsubscribe(IFlightObserver observer);
        void Notify(string details);
    }

    public class Flight : IFlightSubject
    {
        private readonly List<IFlightObserver> _observers = new List<IFlightObserver>();
        public string FlightNumber { get; }
        public FlightStatus Status { get; private set; }
        public string Gate { get; private set; }

        public Flight(string flightNumber, string gate)
        {
            FlightNumber = flightNumber;
            Gate = gate;
            Status = FlightStatus.Scheduled;
        }

        public void Subscribe(IFlightObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                EventLogger.Log($"Рейс {FlightNumber}", $"Підписано отримувача: {observer.Name}");
            }
        }

        public void Unsubscribe(IFlightObserver observer)
        {
            if (_observers.Remove(observer))
            {
                EventLogger.Log($"Рейс {FlightNumber}", $"Відписано отримувача: {observer.Name}");
            }
        }

        public void Notify(string details)
        {
            foreach (var observer in _observers)
            {
                observer.Update(FlightNumber, Status, details);
            }
        }

        public void DelayFlight(int minutes)
        {
            Status = FlightStatus.Delayed;
            string details = $"Рейс затримано на {minutes} хв.";
            EventLogger.Log($"Рейс {FlightNumber}", $"Зміна статусу: {details}");
            Notify(details);
        }

        public void ChangeGate(string newGate)
        {
            Gate = newGate;
            Status = FlightStatus.GateChanged;
            string details = $"Новий вихід на посадку: {newGate}";
            EventLogger.Log($"Рейс {FlightNumber}", $"Зміна статусу: {details}");
            Notify(details);
        }

        public void StartBoarding()
        {
            Status = FlightStatus.BoardingStarted;
            string details = $"Розпочато посадку через вихід {Gate}";
            EventLogger.Log($"Рейс {FlightNumber}", $"Зміна статусу: {details}");
            Notify(details);
        }
    }

    public class AirportDisplayBoard : IFlightObserver
    {
        public string Name => "Інформаційне табло";

        public void Update(string flightNumber, FlightStatus status, string details)
        {
            EventLogger.Log(Name, $"Оновлено рядок для рейсу {flightNumber}: [{status}] {details}");
        }
    }

    public class Passenger : IFlightObserver
    {
        private readonly string _fullName;
        public string Name => $"Пасажир ({_fullName})";

        public Passenger(string fullName)
        {
            _fullName = fullName;
        }

        public void Update(string flightNumber, FlightStatus status, string details)
        {
            EventLogger.Log(Name, $"Отримано SMS щодо рейсу {flightNumber}: {details}");
        }
    }

    public class CateringService : IFlightObserver
    {
        public string Name => "Служба кейтерингу";

        public void Update(string flightNumber, FlightStatus status, string details)
        {
            string action = status == FlightStatus.Delayed
                ? "Перенесено час доставки бортового харчування."
                : $"Зафіксовано зміну статусу рейсу {flightNumber} ({details}).";
            EventLogger.Log(Name, action);
        }
    }

    public class TaxiService : IFlightObserver
    {
        public string Name => "Таксі-сервіс";

        public void Update(string flightNumber, FlightStatus status, string details)
        {
            EventLogger.Log(Name, $"Оновлено прогноз попиту на авто для рейсу {flightNumber}: {details}");
        }
    }

    public class ScenarioRunner
    {
        public static void RunFlightNotificationScenario()
        {
            var flight = new Flight("PS-101", "A4");

            var displayBoard = new AirportDisplayBoard();
            var passenger = new Passenger("Олександр Коваленко");
            var catering = new CateringService();
            var taxi = new TaxiService();

            flight.Subscribe(displayBoard);
            flight.Subscribe(passenger);
            flight.Subscribe(catering);
            flight.DelayFlight(45);

            Console.WriteLine();

            flight.Subscribe(taxi);
            flight.ChangeGate("B12");

            Console.WriteLine();

            flight.Unsubscribe(catering);
            flight.StartBoarding();
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ScenarioRunner.RunFlightNotificationScenario();
        }
    }
}