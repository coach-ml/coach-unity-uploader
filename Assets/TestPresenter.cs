using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReactUnity;
using Coach;

public class TestPresenter : Presenter<TestController, TestModel>
{
    protected override void Render(TestModel model)
    {
        throw new System.NotImplementedException();
    }
}

public class TestController : Controller<TestModel>
{
    public override void Start()
    {
        var coach = new CoachClient();
    }
}

public class TestModel : IModel { }