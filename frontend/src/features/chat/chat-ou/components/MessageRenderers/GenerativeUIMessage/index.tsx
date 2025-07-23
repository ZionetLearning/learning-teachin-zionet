import React from "react";
import type { GenerativeUIMessage as GenerativeUIMessageType } from "../../../types/Message";
import { useStyles } from "./style";
import { LinkMessage, QuizMessage } from "../";

interface GenerativeUIMessageProps {
  message: GenerativeUIMessageType;
}

const componentFactory = (
  componentType: string,
  props: Record<string, any>
) => {
  try {
    switch (componentType) {
      case "LinkMessage":
      case "link":
        if (!props.title || !props.url) {
          return null;
        }
        return <LinkMessage {...(props as any)} />;
      case "QuizMessage":
      case "quiz":
        { if (!props.question || !props.options) {
          return null;
        }

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
    console.error("Error rendering generative UI component:", error);
    return null;
  }
};

const GenerativeUIMessage: React.FC<GenerativeUIMessageProps> = ({
  message,
}) => {
  const classes = useStyles();
  const { componentType, props, fallbackText } = message.content;

  const SpecificComponent = componentFactory(componentType, props);

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
