#Introduction

I present CSharpEval, a lightweight C# Eval and Read-Evaluate-Print-Loop (REPL) program.

This used to be on my own website kamimucode.com, however I have decided the website has out-lived its usefulness and so I am loading this code to Github. I won't be doing any more work on this project (my interests have moved on) so if you want to, feel free to copy it, give it your own name, and go for it!

#What is this?

The CSharpEval source code can be included into any C# program. It gives you the ability to compile the contents of a text string containing some C# code and produce a method delegate. This is done at execution time. This delegate is callable from within the executing program, and the newly compiled code can access any of the public resources of the parent program, such as methods and fields and types.

#Why is this useful?

* Ideal for implementing small Domain Specific Languages (DSL): It allows you to compile from the DSL code to standard C# code, instead of having to compile all the way from the DSL code to Microsoft Intermediate Language (MIL) instructions.
* Great for meta-programming in the .Net environment: Instead of using lots of Reflection calls such as Type.GetField, Type.GetMethod, Activator.CreateInstance, GetValue and Invoke,you simply compose a small C# source code method and evaluate it using CSharpEval to create a dynamic method delegate.
* Extremely handy for debugging tasks which require deep on-the-fly analysis of complex data structures: Instead of using the symbolic debugger to laboriously track through layers of references and manually scanning large arrays, you can run the C# REPL and compose a method in C# to query and present the required data. The call to the REPL dialog can be placed into the source code at the point of interest before compiling the program, or it could be inserted at debug time.
* There are rumors on the Web that version 5 or so of C# will include Compiler-as-a-Service capabilities. Presumably this includes Eval and maybe some sort of REPL. So get yourself a head start!

#How complete is this compiler?

I do not implement the full C# language in CsharpEval; however I have done my best to implement a very useful subset. In particular I have not attempted to implement any of Linq, nor yield return or exception handling. Neither can you define new classes. The standard vanilla features of C# such as statements, expressions, conditionals, loops, and the most commonly used operators, etc are supported.
Getting started

In the following documentation I first introduce the C# REPL program; you can put the C# Eval through its paces and see how well it works. Then the second part discusses the CsharpEval API, and the third part gives complete examples on how to use the CsharpEval code in your programs.

#C# Repl Program

Download the zipped file called c-sharp-rep.zip. This contains an exe file which is an already compiled CsharpEval program that runs the REPL code. Naturally this will only work on a Windows machine. You will also need to have the .NET Framework 3.5 version installed.

So unzip and run this program (no installation is required). You will be presented with an empty edit box. To start things off, enter the universal REPL test:

```
2+2
```

Press shift return (don't forget the shift!) and it will print out 4. Or enter the following (including quotes)

```
"Hello World!"
```

and press shift return.

Let's try a more complicated expression:

```
"Number=" + (20.2 * (3.4 + 5.5) - "A String".Length).ToString()
  + " Date and Time=" + DateTime.Now
```

If there is a syntax error a separate dialog box will pop up telling you of the error. Dismiss this dialog, fix the code and try again. When the code is correct the REPL will compile the method and return with a success message. One possible source of errors is when a unicode character whose index is outside of the standard ascii range is inadvertently used. For example using En dash (code 8211) instead of the minus sign. In such cases you will have to manually edit the input to use the ascii characater.

We shall now code everybody's favourite recursive method:

```
public int Fib(int i)
 {
   if (i == 0) return 0;
   if (i == 1) return 1;
   return Fib(i - 1) + Fib(i - 2);
 }
```

Hint: a limited set of copy and paste editing operations are supported. So copy the above code into the clipboard, then click on the edit box in the REPL window, and press shift-insert. You can also use the mouse to highlight text in the REPL window. Position the mouse onto the text, hold the left button down and drag. Now doing a ctrl-shift will copy the text into the clipboard. Once the text is in the REPL window you do basic editing operations on it. The full range of (say) Visual Studio text editing abilities is not available; but you could always add them yourself:).

Now position the cursor onto any line of this method and press shift enter. It will hopefully be compiled. Enter the expression:

```
Fib(40)
```

And press shift return. Now wait...

On my machine it will print out 102334155 after about 16 seconds. Your mileage may vary. A discussion on timings is given later in this documentation.

Positioning the cursor on any entry, either just typed in or any past entry, and pressing shift return will invoke the REPL evaluation. If the REPL thinks that the code is incomplete (say unmatched brackets) it will do nothing. Otherwise it will attempt to evaluate the code.

So move the cursor back up to one of the edit entries you have just made, possibly modify it, and press shift enter again. The REPL will re-do the evaluation.
Entering a field

To add a field we type (for example)

```
public int NewInt ;
```

The trailing semicolon is important; it signals to the REPL code that this is a field definition and not something else.

CSharpEval compiler will now accept something like

```
public int Increment ( )
 { NewInt = NewInt + 1 ; return NewInt ; }
```

It pretends that NewInt is an instance field. Behind the scenes the NewInt field is an element of a list (more about this in the next part) and each reference to NewInt fetches or stores this element. In this case, since NewInt is a value type, each fetch and store will be accompanied by unboxing and boxing operations.

Test this method by entering:

```
NewInt = 111 ;
  Increment () ;
```

and pressing shift enter. Now type

```
NewInt
```

and press shift enter. The number 112 should be printed.

Note that the statement

```
int NewInt ;
```

will also be accepted but it will not appear to do anything. This is because, without the public keyword in front of it, the REPL assumes this is a statement. REPL will take this statement, place it inside a method and compile and execute the method. Since this method has only a single local variable and does nothing else, then nothing will happen.

#Some implementation detail

CSharpEval can compile stand-alone methods. It can also compile a group of methods and fields and pretend that they are part of an existing class. An instance of this existing class is supplied by the calling code when calling the CSharpEval API.

In the REPL program, the class instance used is the class that implements the REPL loop itself. So the code you are entering into the REPL window can directly access the REPL internals. Some examples that do this are now given.
Timing

Enter the text

```
StopwatchOn
```

And press shift return. The word False will appear on the next line and a cursor will then move to a new edit box. This represents the current value of the StopwatchOn field in the REPL module.

Now enter

```
StopwatchOn = true ;
```

And press shift return. This will set the field to true, and so it will now print out a line stating how long in microseconds it took to evaluate. It then prints out Done to say it has done the statement just entered.

Now enter the following:

```
public string IsStopwatchOn ()
 {
   return StopwatchOn ? "Yes" : "No" ;
 }
```

and press shift return. Now type in the expression

```
IsStopwatchOn()
```

without a semicolon. On pressing shift return the text Yes should be displayed.

You could go back to the edit box with this method text in it, and alter the return statement, perhaps to

```
return StopwatchOn ?
   "Yes, it is on" :
   "No, you just turned it off" ;
```

Press shift return and the method will be updated to this new code. Now go to the IsStopwatchOn() expression and press shift return again. The new message should be printed. You can turn off the StopwatchOn flag and test again to see the expected result.

#Summary so far

* To enter an expression for immediate evaluation, type the expression and press shift return. The expression may run across more than one line. Do not terminate the expression with a semicolon.
* To enter a statement for immediate evaluation, type the statement and terminate with a semicolon. Then press shift return. Multiple statements may be typed in at once, and may run over more than one line.
* To enter a field declaration, type it in as a Type and the name, then terminate with a semicolon. A preceding public or private keyword is required. Then press shift return. If the initial public or private key word is not used, then the REPL will assume this is a statement, and will appear to do nothing.
* To enter a method, type in the full method, always starting with either a private or public keyword. Then press shift return. The REPL editor will count open and close curly brackets and it will only compile the method when all of the brackets are properly matching.
* Multiple fields and methods can be entered into the one edit box. Just enter them and when finished, press shift return.

#Saving and restoring your work

The user can save and restore the contents of the REPL window, and can also copy the contents to a .cs file. The following commands at the REPL command prompt are provided:

```
SaveImage ( some_file_name );
```

This will save the current contents of the REPL window to the given file.

```
RestoreImage ( some_file_name) ;
```

This will restore the contents from the file to the REPL window, erasing the current contents.

Note that this only restores the declared methods and fields, and the contents of the edit boxes in the REPL window. It will not fill in the field values, so they will all be either null or 0.

```
SaveCode ( some_file_name);
```

This writes the fields and methods to the filename in standard C# format.

The REPL program is mainly present as a means of testing the Eval compiler, although as I point out in the introduction it can also be used for debugging program data. It is probably too limited to be able to be used for writing much production code. However, assuming that the nature of the programming task lends itself to this pattern, you could make use of the above commands and use the REPL to write and incrementally test a C# module.

When it is working to some degree you would use SaveCode () to write the new source to a file, then copy and paste this into the final program.

#How fast is this code?

In the above a Fibonacci method was used as an example. On my machine it takes about 15 seconds to do Fib(40). Just by chance there is a Fib method in the CSharpEval source code. It is in the TestProgram class of the full test version. It is a public static method, and so we can call it from the REPL code:

```
TestProgram.Fib(40)
```

So pressing Enter here will result in this statement being compiled into a method, and executed. This code will call the Fib method in the parent program. On my machine this call takes 11.4 seconds in debug mode, and 1.4 seconds in optimized mode, and, of course, it generates the same number.

Let's optimize the first Fib function by making it iterative:

```
public int Fib ( int n )
 {
   if (n<= 1) return n ;
   int last = 1 ;
   int lastlast = 0 ;
   for ( var i = 2 ; i <= n ; i++ ) {
     int temp = lastlast + last ;
     lastlast = last ;
     last = temp ;
   }
   return last ;
 }
```

Copy and paste this into a new edit box in the REPL, and run it. It should generate the same result but only take 6.4 milliseconds. Running the FibIterative method already supplied in the TestProgram class takes about 0.7 millseconds.

Finally, enter the following method:

```
public long TestTiming ( int n )
 {
   long total = 0 ;
   for ( var i = 0 ; i<n ; i++ ) {
     total += i * 1000 + i / 10 + 50 * i ;
   }
   return total ;
  }
```

and press shift enter. Then enter the statement

```
TestTiming ( 100000000 ) ;
```

It will take (on my machine) about 2.1 seconds. Running the same routine which has been placed into the TestProgram class, as in

```
TestProgram.TestTiming ( 100000000 ) ;
```

takes (on my machine with the compiler optimize flag on) about 1.6 seconds.

You can look at the compiled MIL code produced by the CSharpEval. Just enter Maker.MethodIL ( "TestTiming" ) , with no trailing semicolon, into the REPL window. This generates a listing of the Intermediate Language instructions that CSharpEval produces. You can compare this with the C# compiler generated code by using Reflector. The codes, while arranged differently (I place the loop conditional before the loop body, the C# compiler places it after) look very similar, which explains their similar timings.

So in conclusion:

* CSharpEval compiled methods that makes many calls to other compiled methods will be slower, up to a factor of 10 or so. This most likely results from each call having to access the MethodTable list. This slow-down may be comparable to the slow-down when the C# compiler inserts debug statements into code.
* Code that executes many cycles within a single method can be almost as fast as the proper C# compiled code.

# The API

This part describes the API, while the last part describes detailed examples.

#Some features

* Copyrighted under the MIT open source license, and thus usable in any situation, commercial or not.
* The CSharpEval compiled code has full access to the public types and variables of the parent program.
* Uses dynamic methods and so is garbage collected, unlike using the CodeDomProvider compilation of a separate assembly.
* Small footprint, between 5000 lines of source code in its minimal configuration to about 10000 lines in the full C# Repl configuration. It is light enough to be included into the average .Net program without significant penalty. However it cannot be used in the .Net compact framework, or the .Net micro framework, as they do not support dynamic method compilation.
* Requires the .Net framework version 3.5.
* Does not implement the full C# language; however it does implement a very useful subset. In particular it does not implement any of Linq, yield return or exception handling. The standard vanilla features of C# such as statements, expressions, conditionals, loops, and the most commonly used operators, etc are supported.
* Built on top of the basic CSharpEval system is a Repl, with its own user interface. You can create variables and manipulate them, enter or edit methods and call them. The entered code can be saved to disk and restored.

Of course, nothing is without cost, even if it is free:

* This code is produced by me in my spare time, so it is not quite up to the usual Microsoft's .Net compiler quality standards. There will be bugs (absolute guarantee!). But the full source code is available so anyone can potentially fix the bugs.
* The syntax probably deviates from the officially defined syntax, especially in edge cases.
* The intermediate language emitted is not optimal; however it is not excessively slow either.
* Only publicly accessible members of types can be compiled and used, just like using the normal C# compiler. Thus trying to use the Repl to debug objects by looking at their private fields can be problematical.
* The compiled code cannot be debugged by standard debuggers; you have to use print (or MessageBox.Show) statements.
* In other words this is only a poor-man's version of the anticipated Compiler-as-a-Service; but it is all we might have until C# version 5.0.

#Security considerations

Before we proceed I wish to remind all developers to never ever directly expose the C# Eval interface to anonymous users. Direct evaluation of arbitrary C# code is an excellent vector for un-authorised access to your machine! Make sure that all input from anonymous users is heavily pre-processed to prevent any such attacks.

#Setting up a demonstration C# project with CSharpEval

The steps needed to get an initial stand-alone version running on your machine are:

* Download the source code. This is a zip file containing a loose collection of .cs text files.
* Open your version of Visual Studio 2008 or 2010 and make a new C# project. Select the project kind to be WPF Application. Make sure you are using version 3.5 (or later) of the .Net Framework.
* After the project is created, go to the Solutions Explorer and delete the Window1.xaml files and the App.xaml file. 

The CSharpEval code supplies its own windowing and startup code.

* Into this empty project copy the .cs files from the downloaded zip file.
* Double click on the Properties folder in this project, and in the properties page click on the Build tab. In the build page enter the following text Class;Dialog;Repl;Test into the Conditional compilation symbols edit box.
* Compile and run.
* If you are using some other editor, you will need to take the appropriate steps to end up with a compiled WPF application using these files.

If all is well, the program will first run through its unit tests of its compiler, and then it will bring up the Repl dialog box. If you get to this point then you have a working copy. Now we need to talk about the different configurations that are possible.

#Different configurations

The CSharpEval project code can be configured in several ways. The minimal configuration provides only the compiler to turn the source code of a method into a delegate. The maximal configuration provides unit tests plus the full Repl environment together with dialogs for reporting compilation errors. The user chooses which configuration they want by the use of the conditional compilation symbols.

* The *Class;Dialog;Repl;Test* sets up the full configuration. This uses all of the code in the zip file, except that each configuration has its own program file. This version of the program does all of the unit tests, then runs the Repl environment.
* To run just the Repl without the unit tests, use the conditional compile symbols *Class;Dialog;Repl*. This configuration is what is used as the Repl program in the first article.
* If you do not want to use the Repl in your application, but you do want to be able to create multiple methods that act as if they belong to an existing type, use the symbols *Class;Dialog*.
* If you only want to create individual methods, use the symbol *Dialog*.

In the first two cases above the Dialog symbol is essential. In the second two cases this symbol is optional. If it is used then error reporting is done by the pre-supplied error dialog. If this Dialog symbol is not used then:

* If you want to compile methods to classes but use your own error logging, use the symbol *Class*.
* If you want to compile single methods only and use your own error logging, do not use any conditional compile symbols at all. This last is the smallest configuration.

Each of these configurations has its own Program file, thus allowing the configuration to be a stand-alone program. One of these configurations includes the units tests; normally you would not put these into any production program. The source code for the other four configurations can be placed into a production program. You will need to remove the Program file for each such configuration; the setup code in each has to be copied across to your code and modified to suit your needs.

#The Repl evaluation process

Running the CsharpEval code in the Repl configuration allows you to enter C# code and have it executed immediately. Behind the scenes the Repl program takes your code, surrounds it with some extra code and then compiles and runs it. If the code you type in is an expression, then the Repl will generate the following code:

```
partial class REPL {
   public string DoExpression () {
     return (Expression-goes-here).ToString() ;
   }
 }
```

while a statement is turned into

```
partial class REPL {
   public void DoStatement () {
     Statement-goes-here
   }
 }
```

These methods are then compiled by the CSharpEval and then executed. In the case of the expression, the return result is printed to the Repl window.

Notice the partial class REPL in the above. The basic purpose of the CSharpEval program is to turn source code into stand-alone dynamic methods. However it is very useful to allow such methods to call each other as if they were normal methods of a single .Net Type. There is no direct way of creating dynamic types in the .Net environment (except by compiling separate assemblies). So the CSharpEval program fakes it. Behind the scenes it uses an array of method references to hold the methods of a single type, and it compiles the code so it looks as if the methods are inside a pre-supplied .Net Type. It also allows fields to be added to this type, which is done by putting the field values into an array of objects.

So either of the above methods are compiled to a dynamic method which pretends it is a method of the class REPL. This class happens to be the class that implements the Repl functionality. So the compiled code can directly access the fields and methods of the Repl module. As an example, if the statement

```
StopwatchOn = true ;
```

is typed into the command line, it will turn on the StopwatchOn flag in the Repl class. When this flag is true the Repl will print the time taken to evaluate each Repl request.

The REPL class is used here since that is what implements the Repl and it is most convenient to allow code entered into the Repl window to directly access the Repl internals. When calling the CSharpEval compiler directly in production code, the user programmer can specify any class. The only requirement is that this class must have a field like the following

```
public List<object> Fields ;
```

if the programmer wants to add fields as well as methods. If the programmer does not want to add fields this is not required.

#Using Eval

The main use of this system is to programmatically compose some source code, then compile it and obtain a delegate.

* To do this you need to include the CSharpEval source files into your program. You may either put the files into their own project (as either an application or library), or they may be included as is into one of you project files.
* You need to decide which configuration to use.
* You need to create a TypeParser instance, which specifies the assemblies and namespaces that the CSharpEval compiler will access. More about this below.
* If you are not using the in-built error reporting dialogs, you need to specify your own.
* If you are using any of the Dialog configurations, you need to specify the filename of the file to which the dialog window positions and sizes are persisted.
* If you are using the ability to compile methods and fields to an existing class, you will need to create that class.
* You need to programmatically generate the source code. The CSharpEval only accepts source in the form of lexical tokens, so you compose the source as string fragments or tokens and call upon the LexListBuilder class to generate a LexList.
* Finally you need to call MakeMethod or MakeClass to actually compile your generated source, and then retrieve and use the delegates.

This sounds complicated, but don't worry. In the following article I will provide boiler-plate code for all of this. For now, however, we will continue with the documentation of the API.

#TypeParser

Every real C# program needs to have a list of references specifying which assemblies are to be included into the final compilation. Each source file in a C# program can have a list of using statements, which specify which namespaces are to be used without having to explicitly specify the full namespace name.

The CSharpEval also needs this information. Rather than compile this information in from the source string each time, it is pre-computed and passed to the CSharpEval code as an input parameter.

The references and using information is stored in an instance of a class called TypeParser. A new instance of this is created for each different set of references and using statements you may require. The TypeParser instance is used by the CSharpEval code to actually evaluate types, using the assemblies and namespaces provided.

The following demonstrates the construction of a TypeParser instance:

```
TypeParser parser = new TypeParser(
   Assembly.GetExecutingAssembly(),
   new List<string>()
   {
     "System" ,
     "System.Collections.Generic" ,
     "System.Linq" ,
     "System.Text" ,
     "System.Windows" ,
     "System.Windows.Shapes" ,
     "System.Windows.Controls" ,
     "System.Windows.Media" ,
     "System.IO" ,
     "System.Reflection" ,
     "Kamimu"
   }
 );
```

The first parameter to the TypeParser constructor is an Assembly, which will normally be the currently executing assembly. The TypeParser code will use this assembly, plus all assemblies directly accessed by this assembly, during the compilation process.

The second parameter to the constructor is a list of namespaces. This corresponds to the using statements of a C# file.

The constructed parser instance can now be passed to calls to the CsharpEval API methods. It can also be assigned to a static field in the TypeParser class, as in

```
TypeParser.DefaultParser = parser;
```

This default parser is used by some of the CsharpEval methods.

#Lexical analysis

The CSharpEval compiler takes the source in the form of a LexList instance. A LexList instance is a list of tokens of type LexToken. To build the final LexList the helper class LexListBuilder is used. The typical pattern of use for this builder is

```
LexListBuilder lb = new LexListBuilder();
 lb.Add(some_string);
 lb.Add(another_string).Add(another_string2);
 return lb.ToLexList();
```

where there can be any number of Add's to append more strings. There are some variations on the Add method:

* There are overrides that allow the adding of an existing LexList or existing LexListBuilder instance.
* When adding a string you can specify parameters in the string which will be expanded out to their final values. For example,

```
lb.Add("public int 'FunctionName ( int i )" ,
 "FunctionName", the_Name)
```

Here you are creating the method header of the method to be compiled. Rather than use string concatenation to include the method's name, you can use a parameter name (in this case FunctionName). The single quote mark in front of it indicates to the Lexical stage that this is a parameter. The parameter value will be taken from the remaining arguments to the Add call. These arguments are inserted in pairs, the first is the name of the parameter (without the quote mark) and the second is the expansion value.

Any number of parameters may be used in a single call. A parameter name may be used more than once in the input string, all occurrences of it will be replaced with the same expansion name.

* As well as strings, you can specify Type values for the parameter expansions. For example:

```
LexList MakeTheMethodHeader ( Type theReturnType )
 {
   LexListBuilder lb = new LexListBuilder () ;
   lb.Add ( "public 'Type Fn ( int i ) ",
   "Type" , theReturnType ) ;
   // and so on
   return lb.ToLexList() ;
 }
``` 

Here the programmer wants the return type of the delegate to be the same type as is in the argument theReturnType that is passed into the method. Rather than use the TypeParser instance to generate the text name of this type, then include that into the source (which the TypeParser instance will eventually parse during the compilation process and convert back to a type), the type value is simply supplied as a parameter expansion.

This is very convenient, it ensures that the type you want is what is used, without worrying about whether the TypeParser instance used in the compilation process is set up to reference this type.

* When specifying source code inside text strings, it can become cumbersome to use the double quote escapes. So the LexListBuilder allows quote promotion, where the backward single quote and the normal single quote are used in the text, and the LexListBuilder will promote these quote marks to be normal single quote and a double quote. For example,

```
lb.AddAndPromoteQuotes (
 "string s = 'contents' ; char ch = `a` ; " );
```

Here the string contents will be converted so as to be equivalent to the following call

```
lb.Add (
 "string s = \"contents\" ; char ch = 'a' ; " );
```

and the final string contents will therefore be

string s = "contents" ; char ch = 'a' ;

#Compiling a method

To compile a single method, use the static method MakeMethod.Compile. This method has a single type parameter which specifies the type of the delegate to be returned. It has two actual parameters, the first is the TypeParser instance, the second is the list of lexical tokens which represents the source code. An example is:

```
Func<int,string> fn = MakeMethod<Func<int,string>>.Compile (
 parser , theLexList ) ;
```

The parser contains the TypeParser instance to use, and theLexList contains the lexical list of the source. The delegate is called using the standard C# syntax. For example,

```
String s = fn ( 234 ) ;
```

To compile an expression and run it immediately, use the static method MakeMethod.DoExpression (). For example

```
Double d = MakeMethod.DoExpression<double> ( '23.45 / 32' ) ;
```

This requires the type parameter to specify the return type. The parser instance is not supplied, instead the default parser instance is used. The source is specified as text, not as a LexList.

To compile a statement and run it immediately, use the static method MakeMethod.DoStatement () in the same way. For example,

```
MakeMethod.DoStatement ( "StopwatchOn = true ; " ) ;
```

Note that in this case no type parameter is required.

#Compiling methods of a class

To compile one or more methods and one or more fields that are to act as if they are members of a class, use the type MakeClass. This type is created with two actual parameters: the first is the TypeParser instance, the second is the list of lexical tokens that represents the source code. This instance now contains all of the information that allows a number of method and field definitions to act as if they are members of a class.

To obtain the delegates from this instance, the GetFunc and GetAction methods in this instance are used. The example below will hopefully make this clear.

First we define some variables to hold the delegates

```
Func<TestClass, int> getLength;
 Action<TestClass, int[]> setArray;
 Action<TestClass> actInit;
```

Now we create the MakeClass instance

```
MakeClass mc = new MakeClass(parser, LexList.Get(@"
   partial class TestClass
   {
     public int[] LocalInt ;
     public void SetArray ( int[] input )
     {
       LocalInt = input ;
     }
     public int GetLength () { return LocalInt.Length ; }
   }")).
 GetFunc<TestClass, int>("GetLength", out getLength).
 GetAction<TestClass, int[]>("SetArray", out setArray).
 GetAction<TestClass>("FieldsInitialiser", out actInit);
```

Notice the three calls to GetFunc and GetAction at the end. Each such call returns the class instance, so they can be chained together, or specified separately. Each call specifies the type of the class we are compiling to (in this case it is called TestClass), the name of the method, and an out parameter that collects the delegate.

You will see a method in the above that was not explicitly specified - FieldsInitialiser. In a genuine C# class the fields are initialized automatically by the class itself. FieldsInitialiser does the equivalent for any fields you have added. If you haven't added any, then this call is not necessary. It is necessary after each time you compile in any more fields. This method can be called multiple times and it will only initialize the most recent new fields.

```
TestClass tc = new TestClass();
 actInit(tc);
 int[] thearray = new int[300];
 setArray(tc, thearray);
 int I = getLength(tc) ;
```

And this code demonstrates the delegates in use. It creates an instance of the TestClass, calls the FieldsInitialiser method on it, then uses the two compiled methods.

You need to keep in mind that the TestClass instance does not know anything about these extra methods and fields. You could define two different MakeClass instances using the exact same code, creating the delegates as above. You will now be able to call these two separate groups of delegates and their effects will be totally independent of each other. (Unless, of course, your MakeClass code explicitly changes genuine C# fields in the TestClass instance. In this situation the two different groups will be able to affect each other.)

In the next and last article in this series I present lots of examples.

#Examples

In this article I will give examples of how to use the C# Eval code. For any usage the first thing you will need to do is to choose the configuration. Each of the five configurations is chosen by specifying the appropriate conditional symbols in the project properties. As a refresher, the possible configurations are:

* *Class;Dialog;Repl;Test* sets up the full Repl program plus all unit tests. The unit tests are only useful if you are extending the compiler or you are debugging. In such a case you will use the full program as is.
* *Class;Dialog;Repl* runs only the Repl without unit tests.
* *Class;Dialog* allows the compilation of multiple methods and fields and attaches them to a class instance. This uses the pre-supplied error handling dialogs.
* *Class* allows the compilation of multiple methods and fields and attaches them to a class instance. You need to provide your own error handling code.
* *Dialog* allows only the compilation of individual methods. This uses the pre-supplied error handling dialogs.
* No conditional symbols at all will allow only the compilation of individual methods. You need to provide your own error handling code.

Each configuration has its own program file. For the two Repl configurations this program file will bring up the Repl dialog. For the other four configurations this program file does some tests and then exits. The code in these files gives useful examples of how to use the CsharpEval API.

When incorporating the CsharpEval code into your complete program you have a number of choices:

* Just compile the configuration of your choice as is. It will produce an .exe file, which can then be referenced by your project.
* Remove the program files (those starting with 'Program_') and convert the CSharpEval project into a ClassLibrary (otherwise known as a .DLL file). This can now be referenced by your project.
* Remove the program files, and optionally also remove any other files not wanted by your configuration, and drop the remaining files directly into a sub-directory of your project. You can leave the conditional compilation symbols as they are, or remove them and make sure that the files you are not using have been removed.

So let's do some examples. Note that all of these examples appear as a single file in the download for this article. They have been slightly expanded from the demonstration code that appears in this article; the examples have been placed into methods and arranged as unit tests. The best way to test these is to create a new Visual Studio project containing the CsharpEval code, set the configuration to the full Repl, and also drop into this project the Examples.cs file. Now you can run the Repl, and from the Repl command line explicitly call any of these examples.
Namespace

All of the CsharpEval code files use the Kamimu namespace. It is assumed in all following examples that either a using statement like

```
using Kamimu;
```

is present at the top of the code file, or that your code is inside a

```
namespace Kamimu
 {
   ...
 }
```

namespace scope.

#Setting up error handling

If you are running any configuration with the Dialog conditional symbol, you will firstly require a statement like

```
LexToken.ShowError = (msg, theList) =>
 {
   new LexErrorDialog()
   {
     Message = msg,
     CompilerList = theList,
   }.Show();
 };
```

This needs to be executed once at program initialisation. ShowDialog is a static field in the LexToken class and is called to display any errors that CsharpEval may find.

If you are running a configuration without Dialog, you will need to hook up your own error handling. A possible example is

```
LexToken.ShowError = (msg, theList) =>
 {
   MessageBox.Show (
     msg + "\n" + theList.CodeFormat ,
     "Error found" ) ;
 };
```

Naturally you may alter this to your requirements.

#Creating a TypeParser

You always need at least one TypeParser instance. This specifies to CsharpEval the default namespaces and the referenced assemblies to use. You can have as many as you like, depending upon your requirements. Here is an example:

```
TypeParser parser = new TypeParser(
  Assembly.GetExecutingAssembly(),
  new List<string>()
  {
    "System" ,
    "System.Collections.Generic" ,
    "System.Linq" ,
    "System.Text" ,
    "System.Windows" ,
    "System.Windows.Shapes" ,
    "System.Windows.Controls" ,
    "System.Windows.Media" ,
    "System.IO" ,
    "System.Reflection" ,
    "Kamimu"
  }
 );
```

The first parameter specifies in which assembly or assemblies the TypeParser will search for types. The TypeParser is set up to reference this assembly and all assemblies directly referred to by this assembly. This corresponds to the 'References' section of a Visual Studio project.

The second parameter specifies the default namespaces to use. This corresponds to the 'using' statements at the start of a C# code file.

#Immediate compilation and execution of an expression or statement

CsharpEval can compile and immediately execute the contents of string. For this to work you will need to set the default parser

```
TypeParser.DefaultParser = parser ;
```

in some initialisation code. For this example it is assumed that the parser variable is the one created by the call to new TypeParser in the above example.

Now to compile and execute some expression, simply do (for example)

```
Double d = MakeMethod.DoExpression<double> ( "23.45 / 32" ) ;
```

This is equivalent to the straight C# code

```
Double d = 23.45 / 32 ;
```

Another example:

```
public class Examples
 {
   public static string AStr = "A string" ;
   private void Test ()
   {
     string s2=MakeMethod.DoExpression<string>(
       "Examples.AStr+Examples.AStr");
   }
 }
```

which is equivalent to the straight C# code

```
public class Examples
 {
   public static string AStr = "A string" ;
   private void Test ()
   {
     string s2 = AStr + AStr ;
   }
 }
```

If there is the possiblity of compiler errors (say if the input being compiled is obtained directly from a user), then use a try block:

```
double d ;
 try {
   d = MakeMethod.DoExpression<double> ( "23.45 / 32" ) ;
 } catch ( Exception ex ) {
   ... your error handler code here ...
 }
```

Statements can also be compiled and immediately executed. For example,

```
public string AStr ;
 ...
 MakeMethod.DoStatement (
   "Examples.AStr = \"Some string contents\" ; " ) ;
```

which will produce the equivalent action as

```
public string AStr;
 ...
 AStr = "Some string contents" ;
```

(only slower, of course).

Multiple statements are allowed, as in

```
public List<string> list = new List<string>() {
   "red" , "green" , "red" , "orange" ,
   "yellow" , "red" , "green" } ;
 public int counter = 0 ;

MakeMethod.DoStatement(@"
   Examples.Counter = 0 ;
   foreach ( var s in Examples.ColoursList ) {
     if (s == ""red"") Examples.Counter = Examples.Counter + 1 ;
   }"
 );
```

After execution the counter variable should end up with 3 in it. Note that the two variables are assumed to be declared as instance (or static) fields in the enclosing class. CsharpEval cannot access directly the local variables of a method.

#Compiling a single method to produce a delegate

When compiling a method to a delegate, you need to specify the method's signature as a generic type to the CsharpEval MakeMethod class. Here is an example of a function that takes an int parameter and returns a string:

```
Func<int, string> fn = MakeMethod<Func<int, string>>.
   Compile(parser, LexList.Get(@"
     public string TheMethod ( int i )
       {
         return i.ToString() ;
       }"
   ));
```

This can now be called, as in

```
string s = fn ( 123 ) ;
```

or as in

```
List<int> listOfIntegers = new List<int>() {
  99, 120, 4, 134, 18, 19, 200 };
 List<string> list =
  (from i
   in listOfIntegers
   where i > 100
   select fn(i)
 ).ToList();
```

In other words, the delegate can be used like any other delegate in C#.

Another example, this time for a method that takes an int and a double and does not return any value:

```
public static string StrProperty { get ; set ; }
 // In the enclosing class definition

Action<int,double> act = MakeMethod<Action<int,double>>.
  Compile ( parser , LexList.Get ( @"
    public void TheMethod ( int i , double d )
    {
      if (Examples.StrProperty != null &&
          Examples.StrProperty != """")
      {
        Examples.StrProperty =
          Examples.StrProperty + "","" ;
      }
      Examples.StrProperty =
        Examples.StrProperty + i.ToString() +
        "","" + d.ToString() ;
    }"
   )
 ) ;
```

This might be called as in

```
 act ( 1 , 23.4) ;
 act ( 2 , 45.23) ;
 act ( 99 , 1.23) ;
```

After these three calls the contents of StrField should be

```
1,23.4,2,45.23,99,1.23
```

And, as in one of the examples above, you can surround the MakeMethod.Compile call with try brackets if there is any chance that the compilation may fail.

#Different ways of generating a LexList

The MakeMethod.Compile method used above expects a LexList as its parameter.

A LexList can be generated by:

* Calling the static method LexList.Get ( string ). For example,

```
 LexList ll = LexList.Get ( @"
   public str Convert ( int i )
   {
     return i.ToString() ;
   }"
 ) ;
```

There are several overloads for this method. One of these is the static method LexList.Get ( string[] ). For example,

```
string[] lines = new string[] {
   "public str Convert ( int i )" ,
   "{" ,
   "  return i.ToString() ; ",
   "}" } ;
 LexList ll = LexList.Get ( lines ) ;
```

Another overload will take a List<string> parameter, and another will take a ReadOnlyCollection<string> parameter.

* A LexList may also be constructed using the LexListBuilder class. This allows a LexList to be built up piece by piece. An example is

```
public LexList MakeTheMethod ( string nameA )
 {
   LexListBuilder llb = new LexListBuilder () ;
   llb.Add ( "public int GetValue()" ) ;
   llb.Add ( "{ return Examples." + nameA + "; }" ) ;
   return llb.ToLexList() ;
 }
```

So now calling MakeTheMethod ( "A" ) will return a LexList identical to the one returned by

```
LexList.Get ( @"public int GetValue ()
 { return Examples.A ; }" ) ;
```

* The LexListBuilder provides parameter substitution. For example,

```
public LexList MakeTheMethod ( string nameA  )
 {
   LexListBuilder llb = new LexListBuilder () ;
   llb.Add ( "public int GetValue()" ) ;
   llb.Add ( "{ return 'AA ; }" ,
     "AA" , nameA ) ;
   return llb.ToLexList() ;
 }
```

The identifier called AA in the string has a single quote in front of it. This indicates that it is to be used as a substitution parameter. The last two arguments to the Get call specify this parameter name and the value to substitute for it. In this example the 'AA will be replaced by the contents of nameA. So if the above MakeTheMethod is called by

```
MakeTheMethod( "Var1"  ) ;
```

the resulting LexList will be the same as the one returned by the following

```
LexList ll = LexList.Get (
   "public int GetValue () { return Examples.Var1 ; }" ) ;
```

Any number of Parameter name and actual value pairs may be used. If the parameter name is used more than once in the string, all occurences will be replaced. Any parameter name and actual value pair that does not have a corresponding parameter in the string is just ignored. A parameter name must be more than one character long. Another example:

```
public LexList MakeTheMethod (
   string TheName , string TheOtherName )
 {
   LexListBuilder llb = new LexListBuilder () ;
   llb.Add (
     @"public int GetValue ()
       {
         return Alpha.'TheName +
           Alpha.'TheOtherName + Alpha.'TheName ;
       }" ,
     "TheName" , TheName ,
     "TheOtherName" , TheOtherName ) ;
   return llb.ToLexList() ;
 }
```

Calling this with the call

```
MakeTheMethod ( "A" , "B" )
```

will produce a LexList that is identical to the one produced by the following call:

```
LexList.Get (
  @"public int GetValue () {
    return Alpha.A + Alpha.B + Alpha.A ; }" ) ;
```

Note how I have chosen the parameter name, internal to the string argument passed to the LexList.Get method, to be the same as the corresponding name of the MakeTheMethod formal parameter. You might as well do it like this, to avoid having to think up two separate names. As long as you remember that, as far as the C# compiler and the LexList analyser are concerned, they are totally distinct.

Token pasting is supported. Joining a substituted token onto the end of an identifier is done by

```
llb.Add ( "return First'TheName ;" , "TheName" , "Value" ) ;
```

This will produce a LexList equivalent to the one produced by

```
LexList.Get ( "return FirstValue ;" ) ;
```

To join a token onto the front of an identifier, do the following:

```
llb.Add ( "return 'TheName''Last ; " , "TheName" , "Value" ) ;
```

Here the two ' quotes one after the other indicate that the substitution parameter has finished and that the next character is part of a normal identifier. The above example is equivalent to

```
LexList.Get ( "return ValueLast ; " ) ;
```

To join two parameters together:

```
llb.Add ( "return 'NameOne'NameTwo ; " ,
   "NameOne" , "One" , "NameTwo" , "Two" ) ;
```

is equivalent to

```
LexList.Get ( "return OneTwo ;" ) ;
```

Here is a final example of token pasting:

```
llb.Add ( "return First'NameOne'NameTwo'NameThree''Last ; " ,
   "NameOne" , "One" ,
   "NameTwo" , "Two" ,
   "NameThree" , "Three" ) ;
```

which is equivalent to

```
LexList.Get ( "return FirstOneTwoThreeLast ; " ) ;
```

You are not forced to use token substitution, you can just concatenate the strings together using any of the normal C# string handling techniques. However it is useful in making the code easier to understand.

An extension to token substitution is the ability to substitute a type directly into the LexList being constructed. Consider the example

```
llb.Add ( "return typeof (MyClassType) ;" ) ;
```

Using this you are relying on the TypeParser instance having access to the correct MyClassType type. Sometimes this may not always be possible or reliable. For example, MyClassType may be in a different assembly or namespace from the ones accessible in the TypeParser instance. Or you may have several MyClassType's in different namespaces. Or the type you want to use may need to be specified by a long sequence of namespace and class type identifiers. To remove all ambiguity you can substitute in the type directly, as in

```
llb.Add ( "return typeof(`Type) ;",
   "Type" , typeof(MyClassType) ) ;
```

This will expand to the same line as the previous example. The difference is that you are now confident that whatever the type you used as the parameter of this call will be the type that the CsharpEval compiler sees when it evaluates the final LexList.

The LexListBuilder Add method will also take a LexListBuilder argument, or a LexList argument. It also provides something called quote promotion. Consider the following:

```
LexListBuilder llb = new LexListBuilder () ;
 llb.Add (
   "string s = \"The string contents\" + 'Variable ; " ,
   "Variable" , ActualValue ) ;
 llb.Add (
   @"string anotherStr = ""Another string contents"" ; ") ;
 llb.Add ( "char ch = 'c' ; " ) ;
 ... and so on ...
```

Sometimes it is desired to avoid all of that escaping for the double quotes. So the following can be used in its place:

```
LexListBuilder llb = new LexListBuilder () ;
 llb.AddAndPromoteQuotes (
   "string s = 'The string contents' + `Variable ; " ,
   "Variable" , ActualValue ) ;
 llb.AddAndPromoteQuotes (
   "string anotherStr = 'Another string contents' ;" ) ;
 llb.AddAndPromoteQuotes ( "char ch = `c` ; " ) ;
 ... and so on ...
```

The AddAndPromoteQuotes method will promote the ' quote to a " quote, and the ` quote to a ' quote. This quote promotion is purely local to this method call, so you are free to mix calls to Add and to AddAndPromoteQuotes. The final LexList will have the normal ' and " quotes.
Compiling methods and fields and attaching them to a class instance

To attach new methods and fields to an existing class requires the Class compiler symbol to be used.

For these examples we shall be using the following example class:

```
public class TestClass
 {
   public int TheInt;
   public string TheString;
   public int GetTheInt()
   {
     return TheInt;
   }
   public List<object> Fields;
 }
```

The following example adds two new methods to this:

```
MakeClass mc = new MakeClass(parser, LexList.Get(@"
 partial class Examples.TestClass
 {
   public int GetTwoTimesTheInt ()
   {
     return TheInt * 2 ;
   }
   public void SetTheString ( string s )
   {
     TheString = s ;
   }
 }"));
```

This will create the two new methods. We now need to obtain the delegates to these methods. To do this we first declare the delegates (either as local variables or as fields):

```
Func<TestClass,int> GetTwoTimesTheInt ;
 Action<TestClass,string> SetTheString ;
```

Notice how the TestClass type is mentioned explicitly in the signatures.

Then we obtain the delegate values from the mc variable:

```
 mc.GetFunc<TestClass,int>(
  "GetTwoTimesTheInt", out GetTwoTimesTheInt ) ;
 mc.GetAction<TestClass,string>(
  "SetTheString",out SetTheString ) ;
```

At this point the delegate variables GetTwoTimesTheInt and SetTheString have been assigned valid delegates and can be used. For example,

```
TestClass tc = new TestClass () ;
 tc.TheInt = 34 ;
 int i = GetTwoTimesTheInt ( tc ) ;
   // i is now 68 ;
 SetTheString ( tc, "New string value" ) ;
   // tc.TheString now has "New string value"
```

The GetFunc and the GetAction methods return the MakeClass instance. So it is possible to run together the calls:

```
mc.
  GetFunc<TestClass,int>(
    "GetTwoTimesTheInt", out GetTwoTimesTheInt ).
  GetAction<TestClass,string>(
    "SetTheString",out SetTheString ) ;
```

It is therefore also possible to attach these calls to the instantiation of the MakeClass instance, as in

```
MakeClass mc = new MakeClass(parser, LexList.Get(@"
 partial class Examples.TestClass
 {
   public int GetTwoTimesTheInt ()
   {
     return TheInt * 2 ;
   }
   public void SetTheString ( string s )
   {
     TheString = s ;
   }
 }")).
 GetFunc<TestClass, int>(
   "GetTwoTimesTheInt", out GetTwoTimesTheInt).
 GetAction<TestClass, string>(
   "SetTheString", out SetTheString);
```

The methods you add can access any public methods and fields and properties of the real TestClass instance. They can also access any of the attached methods (and fields). You can also attach some methods and then at a later point attach some more. Thus

```
Action<TestClass, string, int> SetStringAndInt;
 mc.AddMethodsAndFields(LexList.Get(@"
   partial class Examples.TestClass
   {
     public void SetStringAndInt ( string s , int i )
     {
       TheInt = i ;
       SetTheString ( s ) ;
     }
   }"), true).
 GetAction<TestClass, string, int>(
   "SetStringAndInt", out SetStringAndInt);
```

Calling the SetStringAndInt delegate will in turn call the SetTheString delegate as part of its actions.

You can provide a new implementation for an already existing method, as long as the method signature is the same. It needs to be the same since I have not implemented overloading of methods. So taking the above example, you can redefine the SetStringInt method with, for example,

```
mc.AddMethodsAndFields(LexList.Get(@"
 partial class Examples.TestClass
 {
   public void SetStringAndInt ( string s , int i )
   {
     TheInt = i * 100 ;
     SetTheString ( s ) ;
   }
 }"), true);
```

Since the delegate already exists, there is no need to fetch it again. The true parameter at the end of each call to AddMethodsAndFields allows methods to be redefined. If this is false, then an attempt to redefine an existing method will generate an exception.

To attach new fields to the TestClass, you need to first add a List<object> Fields declaration to the real TestClass code. So the new TestClass we will be using is

```
public class TestClass
 {
   public int TheInt ;
   public string TheString ;
   public int GetTheInt ()
   {
     return TheInt ;
   }
   public List<object> Fields ;
 }
```

And now adding a new field is easily done:

```
Func<TestClass, int> GetIntValue;
 Action<TestClass> Init;
 MakeClass mc = new MakeClass(parser, LexList.Get(@"
   partial class Examples.TestClass
   {
     public int LastIntValue  ;
     public int GetIntValue ()
     {
       LastIntValue = TheInt ;
       return TheInt ;
     }
   }")).
   GetFunc<TestClass, int>("GetIntValue", out GetIntValue).
   GetAction<TestClass>("FieldsInitialiser", out Init);
```

Notice the appearance of the FieldsInitialiser action. This is automatically supplied to the MakeClass code and must be called at least once for every instance of TestClass we are using the newly added methods on. It will initialise the contents of the Fields list in the real TestClass.

So to use this GetIntValue delegate:

```
TestClass tc1 = new TestClass () ;
 tc1.TheInt = 22 ;
 Init(tc1) ;
 int i = GetIntValue(tc1) ;
   // i is 22 and so is LastIntValue
 tc1.TheInt = 33 ;
 int j = GetIntValue(tc1) ;
   // j is 33 and so is LastIntValue
 TestClass tc2 = new TestClass () ;
 Init(tc2) ;
 tc2.TheInt = 100 ;
 int k = GetIntValue ( tc2) ;
   // k is 100 and so is LastIntValue
```

Note that the Init delegate was called for each instance of TestClass. Each instance has its own Fields list, and each must be initialised.

Having defined one method and field for this MakeClass instance, you can add more:

```
Action<TestClass, string> AddString;
 mc.AddMethodsAndFields(LexList.Get(@"
   partial class Examples.TestClass
   {
     public List<string> ListOfStrings ;
     public void AddString ( string s )
     {
       if (ListOfStrings == null)
         ListOfStrings = new List<string> () ;
       ListOfStrings.Add ( s ) ;
     }
   }"), true).
   GetAction<TestClass, string>("AddString", out AddString);
```

Remember, you must call the FieldsInitialiser method after every time a new field is added to the class, and this must be called for every instance of the class you are using.

#License

This readme text is under the Creative Commons Zero license. Source codes are under the MIT license.
