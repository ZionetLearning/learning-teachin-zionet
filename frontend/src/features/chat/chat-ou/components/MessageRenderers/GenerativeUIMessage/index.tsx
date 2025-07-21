import React from "react";
import type { GenerativeUIMessage as GenerativeUIMessageType } from "../../../types/Message";
import { useStyles } from "./style";
import { LinkMessage } from "../LinkMessage";
import { QuizMessage } from "../QuizMessage";

interface GenerativeUIMessageProps {
  message: GenerativeUIMessageType;
}

// Component factory for generative UI components
const componentFactory = (
  componentType: string,
  props: Record<string, any>
) => {
  try {
    switch (componentType) {
      case "LinkMessage":
      case "link":
        // Ensure required props are present for LinkMessage
        if (!props.title || !props.url) {
          return null;
        }
        return <LinkMessage {...(props as any)} />;
      case "QuizMessage":
      case "quiz":
        // Ensure required props are present for QuizMessage
        { if (!props.question || !props.options) {
          return null;
        }

        // Transform the data structure for QuizMessage component
        const transformedQuizProps = {
          question: props.question,
          explanation: props.explanation,
          options: Array.isArray(props.options)
            ? props.options.map((option: string, index: number) => ({
                id: `option-${index}`,
                text: option,
                isCorrect: index === props.correctAnswer,
              }))
            : [],
          allowMultiple: false,
        };

        return <QuizMessage {...transformedQuizProps} />; }
      default:
        return null;
    }
  } catch (error) {
    // If component fails to render, return null to show fallback
    console.error("Error rendering generative UI component:", error);
    return null;
  }
};

const GenerativeUIMessage: React.FC<GenerativeUIMessageProps> = ({
  message,
}) => {
  const classes = useStyles();
  const { componentType, props, fallbackText } = message.content;

  // Try to render the specific component
  const SpecificComponent = componentFactory(componentType, props);

  // If component is not supported or fails to render, show fallback
  if (!SpecificComponent) {
    return (
      <div className={classes.container}>
        <div className={classes.fallbackText}>{fallbackText}</div>
        <div className={classes.componentInfo}>
          Unsupported component: {componentType}
        </div>
      </div>
    );
  }

  return <div className={classes.container}>{SpecificComponent}</div>;
};

export { GenerativeUIMessage };
