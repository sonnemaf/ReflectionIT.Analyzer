using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionIT.Analyzer.Refactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestClass]
    public class AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChangedTest : CSharpCodeRefactoringTestBase
    {
        [TestMethod]
        public void TestString()
        {
            Test<AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged>(@"
class TestClass
{
    public string $Test { get; set; } 
}", @"
class TestClass
{
    private string _test;

    public string Test
    {
        get
        {
            return _test;
        }

        set
        {
            if (value != _test)
            {
                _test = value;
                OnPropertyChanged(nameof(this.Test));
            }
        }
    }
}");
        }

        [TestMethod]
        public void TestStringWithInitializer() {
            Test<AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged>(@"
class TestClass
{
    public string $Test { get; set; } = ""bla""
}", @"
class TestClass
{
    private string _test = ""bla"";

    public string Test
    {
        get
        {
            return _test;
        }

        set
        {
            if (value != _test)
            {
                _test = value;
                OnPropertyChanged(nameof(this.Test));
            }
        }
    }
}");
        }

        [TestMethod]
        public void TestInternalProperty() {
            Test<AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged>(@"
class TestClass
{
    internal string $Test { get; set; } = ""bla""
}", @"
class TestClass
{
    private string _test = ""bla"";

    internal string Test
    {
        get
        {
            return _test;
        }

        set
        {
            if (value != _test)
            {
                _test = value;
                OnPropertyChanged(nameof(this.Test));
            }
        }
    }
}");
        }

        [TestMethod]
        public void TestProtectedProperty() {
            Test<AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged>(@"
class TestClass
{
    protected string $Test { get; set; } = ""bla""
}", @"
class TestClass
{
    private string _test = ""bla"";

    protected string Test
    {
        get
        {
            return _test;
        }

        set
        {
            if (value != _test)
            {
                _test = value;
                OnPropertyChanged(nameof(this.Test));
            }
        }
    }
}");
        }

        [TestMethod]
        public void TestPrivateSetProperty() {
            Test<AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged>(@"
class TestClass
{
    public string $Test { get; private set; } = ""bla""
}", @"
class TestClass
{
    private string _test = ""bla"";

    public string Test
    {
        get
        {
            return _test;
        }

        private set
        {
            if (value != _test)
            {
                _test = value;
                OnPropertyChanged(nameof(this.Test));
            }
        }
    }
}");
        }

        [TestMethod]
        [ExpectedException(typeof(ActionNotAvailableException))]
        public void TestReadOnlyProperty() {
            Test<AutoPropertyToPrivateFieldWithPropertyAndOnPropertyChanged>(@"
class TestClass
{
    public string $Test { get; } = ""bla""
}", "");
        }
    }
}
