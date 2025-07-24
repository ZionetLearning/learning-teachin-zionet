import React from "react";
import type { GenerativeUIMessage as GenerativeUIMessageType } from "../../../types/Message";
import { useStyles } from "./style";
import { LinkMessage, QuizMessage } from "../";

interface LinkMessageProps extends Record<string, unknown> {
  title: string;
  url: string;
}

interface QuizMessageProps {
  question: string;
  explanation?: string;
  options: Array<{
    id: string;
    text: string;
    isCorrect: boolean;
  }>;
  allowMultiple: boolean;
}

interface RawQuizProps extends Record<string, unknown> {
  question: string;
  explanation?: string;
  options: string[];
  correctAnswer: number;
}

// Type guards
const isLinkProps = (
  props: Record<string, unknown>,
): props is LinkMessageProps => {
  return typeof props.title === "string" && typeof props.url === "string";
};

const isRawQuizProps = (
  props: Record<string, unknown>,
): props is RawQuizProps => {
  return (
    typeof props.question === "string" &&
    Array.isArray(props.options) &&
    typeof props.correctAnswer === "number"
  );
};

interface GenerativeUIMessageProps {
  message: GenerativeUIMessageType;
}

const componentFactory = (
  componentType: string,
  props: Record<string, unknown>,
) => {
  try {
    switch (componentType) {
      case "LinkMessage":
      case "link":
        if (!isLinkProps(props)) {
          return null;
        }
        return <LinkMessage {...props} />;
      case "QuizMessage":
      case "quiz": {
        if (!isRawQuizProps(props)) {
          return null;
        }

        const transformedQuizProps: QuizMessageProps = {
          question: props.question,
          explanation: props.explanation,
          options: props.options.map((option: string, index: number) => ({
            id: `option-${index}`,
            text: option,
            isCorrect: index === props.correctAnswer,
          })),
          allowMultiple: false,
        };

        return <QuizMessage {...transformedQuizProps} />;
      }
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
