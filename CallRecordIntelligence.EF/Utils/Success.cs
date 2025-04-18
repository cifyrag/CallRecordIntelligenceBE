namespace CallRecordIntelligence.EF.Utils;

public sealed record Success
{
	private Success() {}

	public static readonly Success Instance = new();
}
