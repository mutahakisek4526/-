using System.Threading;
using System.Threading.Tasks;

namespace AacV1.Core;

public interface ISpeechService
{
    Task SpeakLatestAsync(string text, CancellationToken ct = default);
    void Stop();
}
