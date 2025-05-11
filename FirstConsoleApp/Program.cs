// using System;

// class Program
// {
//     static void Main()
//     {
//         Console.WriteLine("Sum: " + MathHelper.Add(3, 5));
//         int a=20, b=30;
//         float c=23.326F;
//         User user = new User(name: "Sagar", age: 27);
//         Console.WriteLine("Float: "+ c +" With int vaL: "+(int)c);
//         Console.WriteLine("The first number: "+a+" and 2nd number: "+b);
//         Console.WriteLine("char to int: "+(int)'u');
//         user.PrintInfo();
//         Console.Read();
//     }
// }


using System;
using Newtonsoft.Json;

namespace JsonExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an example object
            var person = new
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30
            };

            // Convert the object to JSON string
            string json = JsonConvert.SerializeObject(person, Formatting.Indented);
            Console.WriteLine("Serialized JSON:\n" + json);

            // Deserialize the JSON string back to an object
            var deserializedPerson = JsonConvert.DeserializeObject(json);
            Console.WriteLine("\nDeserialized Object:\n" + deserializedPerson);
        }
    }
}
