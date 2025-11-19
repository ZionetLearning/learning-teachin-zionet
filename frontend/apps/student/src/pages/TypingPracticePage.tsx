import { useLocation } from "react-router-dom";
import { TypingPractice } from "../features";

export const TypingPracticePage = () => {
  const location = useLocation();
  const retryData = location.state?.retryData;

  return <TypingPractice retryData={retryData} />;
};
