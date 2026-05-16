# /git-merge

Unity 프로젝트의 현재 작업 브랜치를 커밋하고 원격 Develop 브랜치에 머지하는 명령어.

## 실행 순서

1. **Unity 씬 저장**
   - `unity-cli menu "File/Save Project"` 실행

2. **에러 확인**
   - `unity-cli console --type error` 로 컴파일 에러 확인
   - 에러가 존재하면 **즉시 작업 중지**하고 사용자에게 에러 내용 알림

3. **.claude repo 커밋**
   - `git -C "C:/Users/kim05/.claude" status` 로 변경사항 확인
   - 변경사항이 있으면:
     ```bash
     git -C "C:/Users/kim05/.claude" add CLAUDE.md settings.json skills/
     git -C "C:/Users/kim05/.claude" -c user.name="GiJungSUk" -c user.email="kim05220808@gmail.com" commit -m "<브랜치명> 작업 중 변경사항"
     ```
   - 변경사항이 없으면 건너뜀

4. **현재 작업 브랜치에 커밋**
   - `git status` 로 변경사항 확인
   - 커밋 메시지는 **한국어**로, 작업 내용을 요약하고 **날짜와 시간** 포함
     - 형식 예시: `feat: [작업 내용 요약] (2026-04-26 14:30)`
   - `git add` 후 `git commit`

5. **현재 작업 브랜치 원격 푸시**
   - `git push origin <현재 브랜치명>`

6. **원격 Develop 브랜치에 머지**
   - `git checkout Develop`
   - `git pull origin Develop`
   - `git merge --no-ff <작업 브랜치명>`
   - 충돌 발생 시 **즉시 머지 중단**하고 사용자에게 충돌 파일 목록 알림

7. **Develop 원격 푸시**
   - `git push origin Develop`
   - 완료 후 작업 브랜치로 복귀: `git checkout <작업 브랜치명>`
