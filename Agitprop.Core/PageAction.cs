using Agitprop.Core.Enums;

namespace Agitprop.Core;

public record PageAction(PageActionType Type, params object[] Parameters);
