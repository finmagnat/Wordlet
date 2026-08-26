#import <UIKit/UIKit.h>

extern "C" void PlayHaptic(int type)
{
    switch (type)
    {
        case 0:
        {
            UISelectionFeedbackGenerator *generator =
                [[UISelectionFeedbackGenerator alloc] init];
            [generator selectionChanged];
            break;
        }
        case 1:
        case 2:
        case 3:
        {
            UIImpactFeedbackStyle style = type == 1
                ? UIImpactFeedbackStyleLight
                : type == 2
                    ? UIImpactFeedbackStyleMedium
                    : UIImpactFeedbackStyleHeavy;
            UIImpactFeedbackGenerator *generator =
                [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
            [generator impactOccurred];
            break;
        }
        case 4:
        case 5:
        case 6:
        {
            UINotificationFeedbackType feedbackType = type == 4
                ? UINotificationFeedbackTypeSuccess
                : type == 5
                    ? UINotificationFeedbackTypeWarning
                    : UINotificationFeedbackTypeError;
            UINotificationFeedbackGenerator *generator =
                [[UINotificationFeedbackGenerator alloc] init];
            [generator notificationOccurred:feedbackType];
            break;
        }
    }
}
