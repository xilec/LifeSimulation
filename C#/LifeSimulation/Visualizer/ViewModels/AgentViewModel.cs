using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LifeSimulation;
using Visualizer.Annotations;

namespace Visualizer.ViewModels
{
    public class AgentViewModel : ViewModelBase
    {
        private VisualAgentType _type;
        private readonly Agent _agent;

        public AgentViewModel(VisualAgentType type)
        {
            _type = type;
        }

        public AgentViewModel(VisualAgentType type, Agent agent)
        {
            _type = type;
            _agent = agent;
        }

        public VisualAgentType Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged();
                OnPropertyChanged("BackgroundColor");
            }
        }

        public Direction DirectionValue
        {
            get { return _agent.Direction; }
            set
            {
                if (value == _agent.Direction) return;
                _agent.Direction = value;
                OnPropertyChanged();
                OnPropertyChanged("Direction");
            }
        }

        public Brush BackgroundColor
        {
            get
            {
                Brush color;
                switch (Type)
                {
                    default:
                        color = Brushes.LightGray;
                        break;
                }

                return color;
            }
        }

        public Brush ForegroundColor
        {
            get
            {
                Brush color;
                switch (Type)
                {
                    case VisualAgentType.None:
                        color = Brushes.LightGray;
                        break;
                    case VisualAgentType.Plant:
                        color = Brushes.Green;
                        break;
                    case VisualAgentType.Herbivore:
                        color = Brushes.Yellow;
                        break;
                    case VisualAgentType.Carnivore:
                        color = Brushes.Red;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return color;
            }
        }

        public string Direction
        {
            get
            {
                string direction;

                switch (Type)
                {
                    case VisualAgentType.None:
                        direction = string.Empty;
                        break;
                    
                    case VisualAgentType.Plant:
                        direction = "●";
                        break;
                    
                    case VisualAgentType.Herbivore:
                    case VisualAgentType.Carnivore:
                        switch (DirectionValue)
                        {
                            case LifeSimulation.Direction.North:
                                direction = "▲";
                                break;
                            case LifeSimulation.Direction.South:
                                direction = "▼";
                                break;
                            case LifeSimulation.Direction.West:
                                direction = "◀";
                                break;
                            case LifeSimulation.Direction.East:
                                direction = "▶";
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return direction;
            }
        }

        public string Hint
        {
            get
            {
                switch (Type)
                {
                    case VisualAgentType.None:
                        return null;
                    case VisualAgentType.Plant:
                        return "Plant";
                    case VisualAgentType.Herbivore:
                    case VisualAgentType.Carnivore:
                        var hint = Type.ToString() + Environment.NewLine;
                        hint += "Energy: " + _agent.Energy;
                        return hint;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}