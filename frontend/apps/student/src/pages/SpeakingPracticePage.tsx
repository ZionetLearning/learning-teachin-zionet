import { useLocation } from "react-router-dom";
import { SpeakingPractice } from "../features";

export const SpeakingPracticePage = () => {
  const location = useLocation();
  const retryData = location.state?.retryData;

  return (
    <div>
      <SpeakingPractice retryData={retryData} />
    </div>
  );
};
