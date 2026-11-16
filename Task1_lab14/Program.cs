using System;

namespace BukovynaAirport
{
    class Journal
    {
        private int minutes;

        public Journal(int startHour, int startMinute)
        {
            minutes = startHour * 60 + startMinute;
        }

        public void Write(string source, string message)
        {
            int hour = minutes / 60;
            int minute = minutes % 60;
            Console.WriteLine("[" + hour.ToString("00") + ":" + minute.ToString("00") + "] [" + source + "] " + message);
            minutes += 1;
        }
    }

    class GroundCoordinator
    {
        private readonly Journal journal;
        private bool refuelPostponed;

        public GroundService Cleaning { get; private set; }
        public GroundService Catering { get; private set; }
        public GroundService Baggage { get; private set; }
        public GroundService Towing { get; private set; }
        public GroundService Refueling { get; private set; }
        public GroundService Boarding { get; private set; }
        public GroundService Deicing { get; private set; }

        public GroundCoordinator(Journal journal)
        {
            this.journal = journal;
            Cleaning = new CleaningService(this);
            Catering = new CateringService(this);
            Baggage = new BaggageService(this);
            Towing = new TowingService(this);
            Refueling = new RefuelingService(this);
            Boarding = new BoardingService(this);
            Deicing = new DeicingService(this);
        }

        public Journal Journal
        {
            get { return journal; }
        }

        public void OnRequestStart(GroundService service)
        {
            if (service == Refueling && Boarding.IsRunning)
            {
                refuelPostponed = true;
                journal.Write("Координатор", "Ініціатор: " + service.Name + ". Заправку відкладено: триває посадка пасажирів");
                journal.Write(service.Name, "Роботу не розпочато");
                return;
            }

            journal.Write("Координатор", "Ініціатор: " + service.Name + ". Дозволено почати");
            service.StartByCoordinator();
        }

        public void OnFinished(GroundService service)
        {
            if (service == Cleaning)
            {
                journal.Write("Координатор", "Ініціатор: " + service.Name + ". Запущено кейтеринг");
                Catering.StartByCoordinator();
            }
            else if (service == Baggage)
            {
                journal.Write("Координатор", "Ініціатор: " + service.Name + ". Дозволено буксирування");
                Towing.StartByCoordinator();
            }
            else if (service == Boarding && refuelPostponed)
            {
                refuelPostponed = false;
                journal.Write("Координатор", "Ініціатор: " + service.Name + ". Запущено відкладену заправку");
                Refueling.StartByCoordinator();
            }
            else if (service == Refueling)
            {
                journal.Write("Координатор", "Ініціатор: " + service.Name + ". Запущено протиобмерзальну обробку");
                Deicing.StartByCoordinator();
            }
        }
    }

    abstract class GroundService
    {
        private readonly GroundCoordinator coordinator;

        public string Name { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }

        protected GroundService(string name, GroundCoordinator coordinator)
        {
            Name = name;
            this.coordinator = coordinator;
        }

        public void RequestStart()
        {
            coordinator.Journal.Write(Name, "Запит на початок роботи");
            coordinator.OnRequestStart(this);
        }

        public void Finish()
        {
            IsRunning = false;
            IsFinished = true;
            coordinator.Journal.Write(Name, "Роботу завершено");
            coordinator.OnFinished(this);
        }

        public void StartByCoordinator()
        {
            IsRunning = true;
            IsFinished = false;
            coordinator.Journal.Write(Name, "Розпочато роботу");
        }
    }

    class CleaningService : GroundService
    {
        public CleaningService(GroundCoordinator coordinator) : base("Прибирання", coordinator) { }
    }

    class CateringService : GroundService
    {
        public CateringService(GroundCoordinator coordinator) : base("Кейтеринг", coordinator) { }
    }

    class BaggageService : GroundService
    {
        public BaggageService(GroundCoordinator coordinator) : base("Багаж", coordinator) { }
    }

    class TowingService : GroundService
    {
        public TowingService(GroundCoordinator coordinator) : base("Буксирування", coordinator) { }
    }

    class RefuelingService : GroundService
    {
        public RefuelingService(GroundCoordinator coordinator) : base("Заправка", coordinator) { }
    }

    class BoardingService : GroundService
    {
        public BoardingService(GroundCoordinator coordinator) : base("Посадка", coordinator) { }
    }

    class DeicingService : GroundService
    {
        public DeicingService(GroundCoordinator coordinator) : base("Протиобмерзання", coordinator) { }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Situation1();
        }

        static void Situation1()
        {
            Journal journal = new Journal(9, 0);
            GroundCoordinator coordinator = new GroundCoordinator(journal);

            journal.Write("Сценарій", "Прибирання завершується");
            coordinator.Cleaning.Finish();

            journal.Write("Сценарій", "Завантаження багажу завершується");
            coordinator.Baggage.Finish();

            journal.Write("Сценарій", "Заправка під час посадки пасажирів");
            coordinator.Boarding.RequestStart();
            coordinator.Refueling.RequestStart();

            journal.Write("Сценарій", "Посадку завершено");
            coordinator.Boarding.Finish();

            journal.Write("Сценарій", "Заправку завершено, нова служба без змін у інших");
            coordinator.Refueling.Finish();
        }
    }
}
