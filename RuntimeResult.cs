class RuntimeResult
{
    public dynamic value;
    public RuntimeError error;
    public dynamic funcReturnValue;
    public bool loopShouldContinue;
    public bool loopShouldBreak;
    public RuntimeResult()
    {
        Reset();
    }
    void Reset()
    {
        value = null;
        error = null;
        funcReturnValue = null;
        loopShouldContinue = false;
        loopShouldBreak = false;
    }
    public dynamic Register(RuntimeResult res)
    {
        error = res.error;
        funcReturnValue = res.funcReturnValue;
        loopShouldContinue = res.loopShouldContinue;
        loopShouldBreak = res.loopShouldBreak;
        return res.value;
    }
    public RuntimeResult Success(dynamic value)
    {
        Reset();
        this.value = value;
        return this;
    }
    public RuntimeResult SuccessReturn(dynamic value)
    {
        Reset();
        funcReturnValue = value;
        return this;
    }
    public RuntimeResult SuccessContinue()
    {
        Reset();
        loopShouldContinue = true;
        return this;
    }
    public RuntimeResult SuccessBreak()
    {
        Reset();
        loopShouldBreak = true;
        return this;
    }
    public RuntimeResult Failure(RuntimeError error)
    {
        Reset();
        this.error = error;
        return this;
    }
    public bool ShouldReturn { get { return error != null || funcReturnValue != null || loopShouldContinue || loopShouldBreak; } }
}