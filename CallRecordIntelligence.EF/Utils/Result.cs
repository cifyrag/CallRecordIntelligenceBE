using CallRecordIntelligence.EF.Utils;

namespace CallRecordIntelligence.EF.Utils;

public sealed partial record Result<T>
{
	private readonly T? value = default;
	private readonly Error? error = default;

	public bool IsError { get; }
	public T Value => value!;
	public Error Error => error!;

	public Result(T value)
	{
		this.value = value;
		IsError = false;
	}
	
    public Result(Error error)
	{
		this.error = error;
		IsError = true;
	}

	public static implicit operator Result<T>(T value)
		=> new(value);
	
    public static implicit operator Result<T>(Error error)
		=> new(error);
    
}
