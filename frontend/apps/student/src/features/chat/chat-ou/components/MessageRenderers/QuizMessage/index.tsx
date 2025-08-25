import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

interface QuizOption {
  id: string;
  text: string;
  isCorrect?: boolean;
}

interface QuizMessageProps {
  question: string;
  options: QuizOption[];
  explanation?: string;
  allowMultiple?: boolean;
}

const getOptionClassName = (
  status: string,
  classes: Record<string, string>,
) => {
  switch (status) {
    case "correct":
      return classes.optionCorrect;
    case "incorrect":
      return classes.optionIncorrect;
    case "missed":
      return classes.optionMissed;
    default:
      return classes.optionDefault;
  }
};

const QuizMessage: React.FC<QuizMessageProps> = ({
  question,
  options,
  explanation,
  allowMultiple = false,
}) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [selectedOptions, setSelectedOptions] = useState<string[]>([]);
  const [showResults, setShowResults] = useState(false);
  const [hasSubmitted, setHasSubmitted] = useState(false);

  const handleOptionSelect = (optionId: string) => {
    if (hasSubmitted) return;

    if (allowMultiple) {
      setSelectedOptions((prev) =>
        prev.includes(optionId)
          ? prev.filter((id) => id !== optionId)
          : [...prev, optionId],
      );
    } else {
      setSelectedOptions([optionId]);
    }
  };

  const handleSubmit = () => {
    if (selectedOptions.length === 0) return;

    setHasSubmitted(true);
    setShowResults(true);
  };

  const handleReset = () => {
    setSelectedOptions([]);
    setShowResults(false);
    setHasSubmitted(false);
  };

  const getOptionStatus = (option: QuizOption) => {
    if (!showResults) return "default";

    const isSelected = selectedOptions.includes(option.id);
    const isCorrect = option.isCorrect;

    if (isSelected && isCorrect) return "correct";
    if (isSelected && !isCorrect) return "incorrect";
    if (!isSelected && isCorrect) return "missed";
    return "default";
  };

  const getScore = () => {
    const correctOptions = options.filter((opt) => opt.isCorrect);
    const selectedCorrect = selectedOptions.filter(
      (id) => options.find((opt) => opt.id === id)?.isCorrect,
    );

    return {
      correct: selectedCorrect.length,
      total: correctOptions.length,
      percentage: Math.round(
        (selectedCorrect.length / correctOptions.length) * 100,
      ),
    };
  };

  return (
    <div className={classes.container}>
      <div className={classes.question}>{question}</div>

      <div className={classes.options}>
        {options.map((option) => {
          const status = getOptionStatus(option);
          return (
            <div
              key={option.id}
              className={`${classes.option} ${getOptionClassName(status, classes)}`}
              onClick={() => handleOptionSelect(option.id)}
            >
              <div className={classes.optionIndicator}>
                {allowMultiple ? (
                  <div
                    className={`${classes.checkbox} ${selectedOptions.includes(option.id) ? classes.checkboxChecked : ""}`}
                  >
                    {selectedOptions.includes(option.id) && "✓"}
                  </div>
                ) : (
                  <div
                    className={`${classes.radio} ${selectedOptions.includes(option.id) ? classes.radioSelected : ""}`}
                  />
                )}
              </div>
              <div className={classes.optionText}>{option.text}</div>
              {showResults && option.isCorrect && (
                <div className={classes.correctIndicator}>✓</div>
              )}
            </div>
          );
        })}
      </div>

      {!hasSubmitted && (
        <div className={classes.actions}>
          <button
            className={`${classes.button} ${classes.submitButton}`}
            onClick={handleSubmit}
            disabled={selectedOptions.length === 0}
          >
            {t("pages.chatOu.submitAnswer")}
            {allowMultiple && selectedOptions.length > 1 ? "s" : ""}
          </button>
        </div>
      )}

      {showResults && (
        <div className={classes.results}>
          <div className={classes.score}>
            {t("pages.chatOu.score")} {getScore().correct}/{getScore().total} (
            {getScore().percentage}%)
          </div>
          {explanation && (
            <div className={classes.explanation}>
              <strong>{t("pages.chatOu.explanation")}</strong> {explanation}
            </div>
          )}
          <button
            className={`${classes.button} ${classes.resetButton}`}
            onClick={handleReset}
          >
            {t("pages.chatOu.tryAgain")}
          </button>
        </div>
      )}
    </div>
  );
};

export { QuizMessage };
