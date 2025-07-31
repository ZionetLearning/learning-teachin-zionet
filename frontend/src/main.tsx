import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { I18nTranslateProvider } from './providers/i18n-translate-provider';
import { ReactQueryProvider } from './providers';
import './index.css';
import App from './App.tsx';
import { AppInsightsErrorBoundary } from './components';
import { appInsights } from './appInsights';

appInsights.loadAppInsights();

createRoot(document.getElementById('root')!).render(
	<StrictMode>
		<I18nTranslateProvider>
			<ReactQueryProvider>
				<AppInsightsErrorBoundary boundaryName="FrontendRootApp">
					<App />
				</AppInsightsErrorBoundary>
			</ReactQueryProvider>
		</I18nTranslateProvider>
	</StrictMode>
);
