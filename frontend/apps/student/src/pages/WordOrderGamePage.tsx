import { useLocation } from "react-router-dom";
import { WordOrderGame } from "@student/features";

export const WordOrderGamePage = () => {
  const location = useLocation();
  const retryData = location.state?.retryData;

  return <WordOrderGame retryData={retryData} />;
};
