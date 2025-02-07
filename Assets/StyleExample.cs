// USING LINES:
// | Keep using lines at the top of your file.
// | Remove unsed lines.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// NAMESPACES:
// | Use namespaces to ensure that your classes, interfaces, enums, etc. 
// | won’t conflict with existing ones from other namespaces or the global namespace.
// | Use Pascal case, without special symbols or underscores.
// | Add using line at the top to avoid typing namespace repeatedly.
// | Create sub-namespaces with the dot (.) operator, e.g. MyApplication.GameFlow, MyApplication.AI, etc.
// | Some recommend namespaces that reflect the folder structure of the project so it's logically grouped.
// | Strip unused 'usings' except the 'minimally-required set'

namespace UnityCSharpStyleSheetExample
{
    // ENUMS:
    // | Use enums when an object or action can only have one value at a time.
    // | Use Pascal case for enum names and values.
    // | Use a singular noun for the enum name as it represents a single value from a set of possible values. 
    // | They should have no prefix or suffix.
    // | You can place public enums outside of a class to make them global.
    public enum Direction
    {
        North,
        South,
        East,
        West,
    }

    // FLAG ENUM:
    // | Use flag enum to represent combinations of options when multiple values can be chosen at the same time, enabling bitwise operations.
    // | Use a plural noun to indicate the possibility of multiple selections (e.g., AttackModes). 
    // | No prefix or suffix.
    // | Use column-alignment for binary values.
    // | Alternatively, consider using the 1 << butnum style.
    [Flags]
    public enum AttackModes
    {
        // Decimal                         // Binary
        None = 0,                          // 000000
        Melee = 1,                         // 000001
        Ranged = 2,                        // 000010
        Special = 4,                       // 000100

        MeleeAndSpecial = Melee | Special  // 000101
    }

    // INTERFACES:
    // | Interfaces allow you to define a common contract, when unrelated classes need to share common functionality but implement it differently
    // | Prefix interface names with a capital I
    // | Follow this with naming interfaces with adjective phrases that describe the functionality.
     
    public interface IDamageable
    {
        string DamageTypeName { get; }
        float DamageValue { get; }

        bool ApplyDamage(string description, float damage, int numberOfHits);
    }

    public interface IDamageable<T>
    {
        void Damage(T damageTaken);
    }

    // CLASSES or STRUCTS:
    // | Use Pascal case 
    // | Name them with nouns or noun phrases. This distinguishes type names from methods, which are named with verb phrases.
    // | Avoid prefixes.
    // | One Monobehaviour per file. If you have a Monobehaviour in a file, the source file name must match. 
    public class StyleExample : MonoBehaviour
    {

        // FIELDS: 
        // | Avoid special characters (backslashes, symbols, Unicode characters); these can interfere with command line tools.
        // | Use nouns for names, but prefix booleans with a verb.
        // | Use meaningful names. Make names searchable and pronounceable. Don’t abbreviate (unless it’s math).
        // | Use Pascal case for public fields. Use camel case for private variables.
        // | Specify (or omit) the default private access modifier but do it consistently. 
        // | We recommend to leave out things that are implicit and thus redundant (such as private) for simplicity 
        // | if you agree that it doesn't negatively affect ambiguity or readability for you.
        // | There are lot of opinions on the use of prefixes. 
        // | Pick what works best for you and the team and be consistent with your style guide.
        // | You can consider adding an underscore (_) in front of private fields to differentiate from local variables
        // | You can alternatively use more explicit prefixes: m_ = member variable, s_ = static, k_ = const 
        // | favoring readability over brevity as the guiding principle.

        // Some prefer to leave out using any prefix for private fields and specifying the access modifier as below 
        private int elapsedTimeInHours;

        // Some prefer to use an underscore prefix for private fields.
        // int _elapsedTimeInDays;

        // We recommend using explicit prefixes: m_ = member variable which favors specifity and 
        // readability above saving a few keystrokes. However we suggest to leave out "private" as its implicit for simplicity.  
        // Whatever style you choose do it consistently.
        int m_elapsedTimeInSeconds;

        // Use [SerializeField] attribute if you want to display a private field in Inspector.
        // Booleans ask a question that can be answered true or false.
        [SerializeField] bool m_isPlayerDead;

        // Static fields are prefixed with s_
        static int s_sharedCount; 

        // Constants are prefixed with k_
        const int k_maxCount = 100;

        // This groups data from the custom PlayerStats class in the Inspector.
        [SerializeField] PlayerStats m_stats;

        // Use the Range attribute to set minimum and maximum values. 
        // This limits the values to a Range and creates a slider in the Inspector.
        [Range(0f, 1f)] [SerializeField] float m_rangedStat;

        // A tooltip can replace a comment on a serialized field and do double duty.
        [Tooltip("This is another statistic for the player.")]
        [SerializeField] float m_anotherStat;

        // PROPERTIES:
        // | Preferable to a public field.
        // | Pascal case, without special characters.
        // | Use the expression-bodied properties to shorten, but choose your preferrred format.
        // | E.g. use expression-bodied for read-only properties but { get; set; } for everything else.
        // | Use the Auto-Implementated Property for a public property without a backing field.
        // | While you can also use functions to expose private data properties are generally recommended.
        // | For get or set operations involving complex logic or computation, use methods instead of properties.

        // The private backing field
        int m_maxHealth;

        // Read-only, returns backing field
        public int MaxHealthReadOnly => m_maxHealth;

        // Equivalent to: 
        // public int MaxHealth { get; private set; }

        // explicitly implementing getter and setter
        public int MaxHealth
        {
            get => m_maxHealth;
            set => m_maxHealth = value;
        }

        // Write-only (not using backing field)
        public int Health { private get; set; }

        // Write-only, without an explicit setter
        public void SetMaxHealth(int newMaxValue) => m_maxHealth = newMaxValue;

        // Auto-implemented property without backing field
        public string DescriptionName { get; set; } = "Fireball";

        // EVENTS:
        // | Name with a verb phrase.
        // | Present participle means "before" and past participle means "after."
        // | Use System.Action delegate for most events (can take 0 to 16 parameters).
        // | Define a custom EventArg only if necessary (either System.EventArgs or a custom struct).
        // | OR alternatively, use the System.EventHandler; choose one and apply consistently.
        // | Choose a naming scheme for events, event handling methods (subscriber/observer), 
        // | and event raising methods (publisher/subject)
        // | e.g. event/action = "OpeningDoor", event raising method = "OnDoorOpened", event handling method = "MySubject_DoorOpened"
   
        // Event before
        public event Action OpeningDoor;

        // Event after
        public event Action DoorOpened;     

        // Event with int parameter
        public event Action<int> PointsScored;
        
        // Custom event with custom EventArgs
        public event Action<CustomEventArgs> ThingHappened;

        // These are event raising methods, e.g. OnDoorOpened, OnPointsScored, etc.
        // Prefix the event raising method (in the subject) with “On”.
        // Alternatively, event handling method e.g. MySubject_DoorOpened().
        
        public void OnDoorOpened()
        {
            DoorOpened?.Invoke();
        }

        public void OnPointsScored(int points)
        {
            PointsScored?.Invoke(points);
        }

        // This is a custom EventArg made from a struct.
        public struct CustomEventArgs
        {
            public int ObjectID { get; }
            public Color Color { get; }

            public CustomEventArgs(int objectId, Color color)
            {
                this.ObjectID = objectId;
                this.Color = color;
            }
        }

        // METHODS:
        // | While “function” and “method” are often used interchangeably, method is the right term in Unity development
        // | because you can’t write a function without incorporating it into a class in C#.
        // | Start a method name with a verb or verb phrases to show an action. Add context if necessary. e.g. GetDirection, FindTarget, etc.
        // | Methods returning bool should ask questions: Much like Boolean variables themselves.
        // | Prefix methods with a verb if they return a true-false condition. 
        // | This phrases them in the form of a question, e.g. IsGameOver, HasStartedTurn
        // | Use camel case for parameters. Format parameters passed into the method like local variables.

        // | SOME GENERAL TIPS FOR METHODS:
        // | Avoid long methods. If a method is too long, consider breaking it into smaller methods.
        // | Avoid methods with too many parameters. If a method has more than three parameters, 
        // | consider using a class or struct to group them.
        // | Avoid excessive overloading: You can generate an endless permutation of method overloads.
        // | Avoid side effects: A method only needs to do what its name advertises.
        // | A good name for a method reflects what it does.
        // | Avoid setting up methods to work in multiple different modes based on a flag. 
        // | Make two methods with distinct names instead, e.g. GetAngleInDegrees and GetAngleInRadians. 

        // Methods start with a verb.
        public void SetInitialPosition(float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        // Methods ask a question when they return bool.
        public bool IsNewPosition(Vector3 newPosition)
        {
            return (transform.position == newPosition);
        }

        void FormatExamples(int someExpression)
        {
            // VAR:
            // | While avoiding ambiguity and always looking for ways to improve readability, 
            // | you can use var  when the type is clear from the context,
            // | With good naming ambiguity should be less of an issue because variable names already convey the intent.  
            // | Refactoring is simpler with var since it abstracts away the specific type, 
            // | reducing the number of places where code needs to be updated when types change.  
            // | In foreach loops, var ensures that the iteration variable matches the type provided by the enumerator. 
            // | If you explicitly declare a mismatched type, the compiler may allow it, leading to runtime errors.

            var powerUps = new List<PlayerStats>();
            var dict = new Dictionary<string, List<GameObject>>();

            // SWITCH STATEMENTS:
            // | It’s generally advisable to replace longer if-else chains with a switch statement for better readability.
            // | The formatting can vary. Select one for your style guide and follow it.
            // | This example indents each case and the break underneath.
            switch (someExpression)
            {
                case 0:
                    // ..
                    break;
                case 1:
                    // ..
                    break;
                case 2:
                    // ..
                    break;
            }

            // BRACES: 
            // | Where possible, don’t omit braces, even for single-line statements. 
            // | Or avoid single-line statements entirely for debuggability.
            // | Keep braces in nested multi-line statements.

            // This single-line statement keeps the braces...
            for (int i = 0; i < 100; i++) { DoSomething(i); }

            // ... but this is more reable and often more debuggable. 
            for (int i = 0; i < 100; i++)
            {
                DoSomething(i);
            }

            // Separate the statements for readability.
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    DoSomething(j);
                }
            }
        }

        void DoSomething(int x)
        {
            // .. 
        }
    }

    // OTHER CLASSES:
    // | Define as many other helper/non-Monobehaviour classes in your file as needed.
    // | This is a serializable class that groups fields in the Inspector.
    [Serializable]
    public struct PlayerStats
    {
        public int MovementSpeed;
        public int HitPoints;
        public bool HasHealthPotion;
    }

}