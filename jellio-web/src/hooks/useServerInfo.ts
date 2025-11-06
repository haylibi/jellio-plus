import { useEffect, useState } from 'react';
import useAccessToken from '@/hooks/useAccessToken.ts';
import { getServerInfo } from '@/services/backendService.ts';
import type { ServerInfo, Maybe } from '@/types';

const useServerInfo = (): Maybe<ServerInfo> => {
  const accessToken = useAccessToken();
  const [serverInfo, setServerInfo] = useState<ServerInfo | null | undefined>();

  useEffect(() => {
    const fetchServerInfo = async (): Promise<void> => {
      try {
        // Try with token if present; otherwise rely on session cookies
        const info = await getServerInfo(accessToken ?? undefined);
        setServerInfo({
          accessToken: accessToken ?? '',
          ...info,
        });
      } catch (error) {
        console.error('Failed to fetch server info:', error);
        setServerInfo(null);
      }
    };

    void fetchServerInfo();
  }, [accessToken]);

  return serverInfo;
};

export default useServerInfo;
